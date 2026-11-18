<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import type { SettingDefinition } from './api/types'
import ChangeSummaryDialog from './components/ChangeSummaryDialog.vue'
import SettingField from './components/SettingField.vue'
import { useSettings, type ChangeSummary } from './composables/useSettings'

const s = useSettings()

const activeGroup = ref('genel')
const search = ref('')
const onlyChanged = ref(false)
const toasts = ref<{ id: number; ok: boolean; text: string }[]>([])
/** Doluysa "değişiklik özeti" penceresi açıktır. */
const pendingSummary = ref<ChangeSummary[] | null>(null)

const q = computed(() => search.value.trim().toLocaleLowerCase('tr'))
const groups = computed(() => s.catalog.value?.groups ?? [])
const settings = computed(() => s.catalog.value?.settings ?? [])

function matches(def: SettingDefinition) {
  if (!q.value) return true
  const hay = [def.label, def.description, def.key, def.legacyControlId, ...(def.options ?? []).map((o) => o.label)]
    .filter(Boolean)
    .join(' ')
    .toLocaleLowerCase('tr')
  return hay.includes(q.value)
}

/** Settings to show: the active group, or every match when searching / filtering. */
const visible = computed(() => {
  const crossGroup = q.value !== '' || onlyChanged.value
  return settings.value.filter(
    (d) => (crossGroup || d.group === activeGroup.value) && matches(d) && (!onlyChanged.value || s.isDirty(d.key)),
  )
})

const visibleByGroup = computed(() =>
  groups.value
    .map((g) => ({ group: g, items: visible.value.filter((d) => d.group === g.key) }))
    .filter((x) => x.items.length > 0),
)

function groupCount(key: string) {
  return settings.value.filter((d) => d.group === key && matches(d)).length
}
function groupDirty(key: string) {
  return settings.value.filter((d) => d.group === key && s.isDirty(d.key)).length
}

function toast(ok: boolean, text: string) {
  const id = Date.now() + Math.random()
  toasts.value.push({ id, ok, text })
  setTimeout(() => (toasts.value = toasts.value.filter((t) => t.id !== id)), ok ? 3500 : 7000)
}

/** Kaydet'e basılınca önce değişiklik özetini gösterir. */
function requestSave() {
  if (s.saving.value || s.dirtyKeys.value.length === 0) return
  pendingSummary.value = s.describeChanges()
}

/** Kullanıcı özet penceresinde onayladığında gerçekten kaydeder. */
async function confirmSave() {
  const result = await s.save()
  pendingSummary.value = null
  toast(result.ok, result.message)
  if (!result.ok) {
    // jump to the first setting with an error
    const firstKey = Object.keys(s.fieldErrors.value)[0]
    if (firstKey) goTo(firstKey)
  }
}

function discard() {
  if (confirm(`${s.dirtyKeys.value.length} kaydedilmemiş değişiklik geri alınsın mı?`)) s.resetDraft()
}

async function changeRow(event: Event) {
  const next = Number((event.target as HTMLSelectElement).value)
  if (s.dirtyKeys.value.length && !confirm('Kaydedilmemiş değişiklikler kaybolacak. Devam edilsin mi?')) {
    ;(event.target as HTMLSelectElement).value = String(s.row.value)
    return
  }
  await s.load(next)
}

/** Navigate to a setting (used by dependency hints and validation errors). */
async function goTo(key: string) {
  const def = s.byKey.value.get(key)
  if (!def) return
  search.value = ''
  onlyChanged.value = false
  activeGroup.value = def.group
  await nextTick()
  const el = document.getElementById(`setting-${key}`)
  if (!el) return
  el.scrollIntoView({ behavior: 'smooth', block: 'center' })
  el.classList.add('flash')
  setTimeout(() => el.classList.remove('flash'), 1600)
}

function selectGroup(key: string) {
  activeGroup.value = key
  search.value = ''
  onlyChanged.value = false
  window.scrollTo({ top: 0 })
}

function onKey(e: KeyboardEvent) {
  if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
    e.preventDefault()
    requestSave()
  }
}
function onBeforeUnload(e: BeforeUnloadEvent) {
  if (s.dirtyKeys.value.length) e.preventDefault()
}

onMounted(() => {
  s.load()
  window.addEventListener('keydown', onKey)
  window.addEventListener('beforeunload', onBeforeUnload)
})
onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKey)
  window.removeEventListener('beforeunload', onBeforeUnload)
})

const activeTitle = computed(() => groups.value.find((g) => g.key === activeGroup.value))
</script>

<template>
  <div class="app">
    <header class="topbar">
      <div class="brand">
        <h1>Teknik Ayarlar</h1>
        <span class="sub">B2B portalının çalışma ayarları</span>
      </div>

      <div class="search">
        <input
          v-model="search"
          type="search"
          placeholder="Ayar ara… (ör. stok, menü, KDV, ParameterCart)"
          aria-label="Ayarlarda ara"
        />
      </div>

      <div class="top-actions">
        <label class="chip" :class="{ active: onlyChanged }">
          <input v-model="onlyChanged" type="checkbox" />
          Sadece değişenler
          <span v-if="s.dirtyKeys.value.length" class="pill">{{ s.dirtyKeys.value.length }}</span>
        </label>

        <label v-if="s.rows.value.length > 1" class="row-select" title="DealerSettings tablosunda anahtar kolon yok; satır sırasıyla seçilir.">
          Kayıt
          <select :value="s.row.value" @change="changeRow">
            <option v-for="r in s.rows.value" :key="r.row" :value="r.row">
              {{ r.row }}. satır{{ r.dealerName ? ' – ' + r.dealerName : r.logo ? ' – ' + r.logo : '' }}
            </option>
          </select>
        </label>
      </div>
    </header>

    <div v-if="s.loading.value && !s.catalog.value" class="state-box">Ayarlar yükleniyor…</div>

    <div v-else-if="s.loadError.value" class="state-box error">
      <p>{{ s.loadError.value }}</p>
      <button type="button" class="btn" @click="s.load()">Tekrar dene</button>
    </div>

    <div v-else class="layout">
      <nav class="sidebar" aria-label="Ayar grupları">
        <button
          v-for="g in groups"
          :key="g.key"
          type="button"
          class="nav-item"
          :class="{ active: g.key === activeGroup && !q && !onlyChanged, empty: q && !groupCount(g.key) }"
          @click="selectGroup(g.key)"
        >
          <span class="nav-title">{{ g.title }}</span>
          <span class="nav-meta">
            <span v-if="groupDirty(g.key)" class="pill warn">{{ groupDirty(g.key) }}</span>
            <span class="count">{{ groupCount(g.key) }}</span>
          </span>
        </button>
      </nav>

      <main class="content">
        <template v-if="!q && !onlyChanged && activeTitle">
          <div class="group-head">
            <h2>{{ activeTitle.title }}</h2>
            <p>{{ activeTitle.description }}</p>
          </div>
        </template>
        <p v-else-if="visible.length" class="result-info">
          {{ visible.length }} ayar bulundu
        </p>

        <div v-if="!visible.length" class="state-box">
          <template v-if="onlyChanged">Henüz değiştirilmiş bir ayar yok.</template>
          <template v-else>“{{ search }}” için sonuç bulunamadı.</template>
        </div>

        <section v-for="block in visibleByGroup" :key="block.group.key" class="block">
          <h3 v-if="q || onlyChanged" class="block-title">{{ block.group.title }}</h3>
          <div class="list">
            <SettingField
              v-for="def in block.items"
              :key="def.key"
              :def="def"
              :draft="s.draft"
              :original="s.original.value[def.key] ?? null"
              :dirty="s.isDirty(def.key)"
              :errors="s.fieldErrors.value[def.key]"
              :dependency-label="def.dependsOn ? s.byKey.value.get(def.dependsOn.key)?.label : undefined"
              :highlight="q || undefined"
              @go-to="goTo"
            />
          </div>
        </section>
      </main>
    </div>

    <transition name="slide">
      <div v-if="s.dirtyKeys.value.length" class="savebar" role="region" aria-label="Kaydet">
        <span>
          <b>{{ s.dirtyKeys.value.length }}</b> kaydedilmemiş değişiklik
        </span>
        <div class="savebar-actions">
          <button type="button" class="btn ghost" :disabled="s.saving.value" @click="discard">Vazgeç</button>
          <button type="button" class="btn primary" :disabled="s.saving.value" @click="requestSave">
            {{ s.saving.value ? 'Kaydediliyor…' : 'Kaydet' }}
          </button>
        </div>
      </div>
    </transition>

    <ChangeSummaryDialog
      v-if="pendingSummary"
      :changes="pendingSummary"
      :saving="s.saving.value"
      @confirm="confirmSave"
      @cancel="pendingSummary = null"
    />

    <div class="toasts" aria-live="polite">
      <div v-for="t in toasts" :key="t.id" class="toast" :class="t.ok ? 'ok' : 'fail'">{{ t.text }}</div>
    </div>
  </div>
</template>

<style scoped>
.topbar {
  position: sticky;
  top: 0;
  z-index: 20;
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 14px 28px;
  background: var(--c-surface);
  border-bottom: 1px solid var(--c-border);
  flex-wrap: wrap;
}
.brand h1 {
  margin: 0;
  font-size: 20px;
}
.brand .sub {
  font-size: 13px;
  color: var(--c-muted);
}
.search {
  flex: 1 1 320px;
}
.search input {
  width: 100%;
  max-width: 520px;
}
.top-actions {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}
.chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border: 1px solid var(--c-border);
  border-radius: 999px;
  font-size: 13px;
  cursor: pointer;
  user-select: none;
}
.chip input {
  display: none;
}
.chip.active {
  background: var(--c-accent-soft);
  border-color: var(--c-accent);
}
.row-select {
  font-size: 13px;
  display: flex;
  gap: 6px;
  align-items: center;
  color: var(--c-muted);
}
.layout {
  display: grid;
  grid-template-columns: 250px 1fr;
  gap: 28px;
  padding: 24px 28px 120px;
  max-width: 1400px;
  margin: 0 auto;
}
.sidebar {
  position: sticky;
  top: 90px;
  align-self: start;
  display: grid;
  gap: 2px;
}
.nav-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  padding: 9px 12px;
  border: none;
  background: none;
  border-radius: 8px;
  text-align: left;
  cursor: pointer;
  font-size: 14px;
  color: var(--c-text);
}
.nav-item:hover {
  background: var(--c-hover);
}
.nav-item.active {
  background: var(--c-accent-soft);
  color: var(--c-accent-strong);
  font-weight: 600;
}
.nav-item.empty {
  opacity: 0.4;
}
.nav-meta {
  display: flex;
  gap: 6px;
  align-items: center;
}
.count {
  font-size: 12px;
  color: var(--c-muted);
}
.pill {
  font-size: 11px;
  font-weight: 700;
  padding: 1px 7px;
  border-radius: 999px;
  background: var(--c-accent);
  color: #fff;
}
.pill.warn {
  background: var(--c-warn);
}
.group-head h2 {
  margin: 0 0 4px;
  font-size: 22px;
}
.group-head p,
.result-info {
  margin: 0 0 18px;
  color: var(--c-muted);
}
.block + .block {
  margin-top: 26px;
}
.block-title {
  font-size: 13px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--c-muted);
  margin: 0 0 10px;
}
.list {
  display: grid;
  gap: 12px;
}
.state-box {
  padding: 40px;
  text-align: center;
  color: var(--c-muted);
}
.state-box.error {
  color: var(--c-danger);
}
.savebar {
  position: fixed;
  left: 50%;
  bottom: 20px;
  transform: translateX(-50%);
  z-index: 30;
  display: flex;
  align-items: center;
  gap: 24px;
  padding: 12px 14px 12px 20px;
  background: var(--c-text);
  color: var(--c-surface);
  border-radius: 12px;
  box-shadow: 0 10px 30px rgb(0 0 0 / 0.25);
}
.savebar-actions {
  display: flex;
  gap: 8px;
}
.savebar .btn.ghost {
  color: var(--c-surface);
  border-color: rgb(255 255 255 / 0.3);
}
.slide-enter-active,
.slide-leave-active {
  transition: transform 0.2s, opacity 0.2s;
}
.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  transform: translate(-50%, 20px);
}
.toasts {
  position: fixed;
  right: 20px;
  top: 80px;
  z-index: 40;
  display: grid;
  gap: 8px;
}
.toast {
  padding: 10px 14px;
  border-radius: 8px;
  color: #fff;
  font-size: 14px;
  max-width: 380px;
  box-shadow: 0 6px 18px rgb(0 0 0 / 0.15);
}
.toast.ok {
  background: var(--c-success);
}
.toast.fail {
  background: var(--c-danger);
}

@media (max-width: 860px) {
  .topbar {
    padding: 12px 16px;
  }
  .layout {
    grid-template-columns: 1fr;
    padding: 12px 16px 120px;
    gap: 14px;
  }
  .sidebar {
    position: static;
    display: flex;
    overflow-x: auto;
    gap: 6px;
    padding-bottom: 4px;
  }
  .nav-item {
    white-space: nowrap;
    border: 1px solid var(--c-border);
  }
  .savebar {
    left: 12px;
    right: 12px;
    transform: none;
    justify-content: space-between;
  }
  .slide-enter-from,
  .slide-leave-to {
    transform: translateY(20px);
  }
}
</style>
