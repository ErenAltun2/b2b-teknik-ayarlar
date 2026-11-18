import { computed, reactive, ref } from 'vue'
import { api, ApiError } from '../api/client'
import type { RowSummary, SettingChange, SettingDefinition, SettingsCatalog, SettingValues } from '../api/types'

/** Parses a stored bitmask ("", null or a decimal string) into a BigInt. */
export function toBig(value: string | null | undefined): bigint {
  if (value == null || value.trim() === '') return 0n
  try {
    return BigInt(value.trim())
  } catch {
    return 0n
  }
}

export function isOn(value: string | null | undefined) {
  return value === '1'
}

/** Kaydetmeden önce gösterilecek, okunabilir değişiklik bilgisi. */
export interface ChangeSummary {
  key: string
  label: string
  before?: string
  after?: string
  added?: string[]
  removed?: string[]
}

/** Ham değeri kullanıcının anlayacağı metne çevirir ("1" → "Açık", "3" → "Gösterme"). */
export function displayValue(def: SettingDefinition, value: string | null): string {
  if (value == null || value === '') return '(boş)'
  if (def.control === 'switch') return value === '1' ? 'Açık' : 'Kapalı'
  if (def.control === 'select') return def.options?.find((o) => o.value === value)?.label ?? value
  return value
}

/** Is the dependency of a setting satisfied with the current draft values? */
export function dependencyMet(def: SettingDefinition, values: SettingValues) {
  const dep = def.dependsOn
  if (!dep) return true
  const v = values[dep.key] ?? ''
  if (dep.in) return dep.in.includes(v)
  if (dep.notIn) return !dep.notIn.includes(v)
  return true
}

/**
 * Holds the catalog, the values loaded from the API (`original`) and the
 * user's edits (`draft`). Only the difference is sent on save; for bitwise
 * settings only the bits that changed are sent (setBits / clearBits), so the
 * API can merge them without touching any other bit.
 */
export function useSettings() {
  const catalog = ref<SettingsCatalog | null>(null)
  const rows = ref<RowSummary[]>([])
  const row = ref(1)
  const original = ref<SettingValues>({})
  const draft = reactive<SettingValues>({})
  const loading = ref(true)
  const saving = ref(false)
  const loadError = ref<string | null>(null)
  const fieldErrors = ref<Record<string, string[]>>({})

  const byKey = computed(() => new Map((catalog.value?.settings ?? []).map((s) => [s.key, s])))

  function isDirty(key: string) {
    const def = byKey.value.get(key)
    if (!def) return false
    if (def.bitwise) return toBig(draft[key]) !== toBig(original.value[key])
    return (draft[key] ?? null) !== (original.value[key] ?? null)
  }

  const dirtyKeys = computed(() => Object.keys(draft).filter(isDirty))

  function resetDraft() {
    for (const k of Object.keys(draft)) delete draft[k]
    Object.assign(draft, original.value)
    fieldErrors.value = {}
  }

  async function load(selectedRow?: number) {
    loading.value = true
    loadError.value = null
    try {
      if (!catalog.value) {
        const [c, r] = await Promise.all([api.catalog(), api.rows()])
        catalog.value = c
        rows.value = r
      }
      if (selectedRow) row.value = selectedRow
      const data = await api.get(row.value)
      row.value = data.row
      original.value = data.values
      resetDraft()
    } catch (e) {
      loadError.value = e instanceof Error ? e.message : String(e)
    } finally {
      loading.value = false
    }
  }

  function buildChanges(): SettingChange[] {
    return dirtyKeys.value.map((key) => {
      const def = byKey.value.get(key)!
      if (!def.bitwise) return { key, value: draft[key] ?? null }
      const before = toBig(original.value[key])
      const after = toBig(draft[key])
      return {
        key,
        setBits: (after & ~before).toString(),
        clearBits: (before & ~after).toString(),
      }
    })
  }

  /** Değişen her ayar için önce/sonra özeti. Bitwise ayarlarda eklenen ve kaldırılan seçenekleri listeler. */
  function describeChanges(): ChangeSummary[] {
    return dirtyKeys.value.map((key) => {
      const def = byKey.value.get(key)!
      const before = original.value[key] ?? null
      const after = draft[key] ?? null

      if (def.bitwise) {
        const b = toBig(before)
        const a = toBig(after)
        const options = def.options ?? []
        return {
          key,
          label: def.label,
          added: options.filter((o) => (a & BigInt(o.value)) !== 0n && (b & BigInt(o.value)) === 0n).map((o) => o.label),
          removed: options.filter((o) => (b & BigInt(o.value)) !== 0n && (a & BigInt(o.value)) === 0n).map((o) => o.label),
        }
      }
      return { key, label: def.label, before: displayValue(def, before), after: displayValue(def, after) }
    })
  }

  async function save(): Promise<{ ok: boolean; message: string }> {
    const changes = buildChanges()
    if (changes.length === 0) return { ok: true, message: 'Değişiklik yok.' }
    saving.value = true
    fieldErrors.value = {}
    try {
      const result = await api.save(row.value, changes)
      original.value = result.settings.values
      resetDraft()
      return { ok: true, message: `${result.changedKeys.length} ayar kaydedildi.` }
    } catch (e) {
      if (e instanceof ApiError) {
        fieldErrors.value = e.fieldErrors
        return { ok: false, message: e.message }
      }
      return { ok: false, message: String(e) }
    } finally {
      saving.value = false
    }
  }

  return {
    catalog, rows, row, original, draft, loading, saving, loadError, fieldErrors,
    byKey, dirtyKeys, isDirty, load, save, resetDraft, describeChanges,
  }
}
