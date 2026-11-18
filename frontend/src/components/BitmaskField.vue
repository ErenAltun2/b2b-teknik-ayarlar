<script setup lang="ts">
import { computed, ref } from 'vue'
import type { SettingDefinition } from '../api/types'
import { toBig } from '../composables/useSettings'

const props = defineProps<{
  def: SettingDefinition
  modelValue: string | null
  original: string | null
  highlight?: string
}>()
const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const filter = ref('')
const showTech = ref(false)
const expanded = ref(false)

const value = computed(() => toBig(props.modelValue))
const before = computed(() => toBig(props.original))
const options = computed(() => props.def.options ?? [])
const known = computed(() => toBig(props.def.knownMask))

const selectedCount = computed(() => options.value.filter((o) => isChecked(o.value)).length)
/** Bits stored in the DB that no option on the screen represents. They are preserved on save. */
const unknownBits = computed(() => value.value & ~known.value)

const visible = computed(() => {
  const q = (filter.value || props.highlight || '').toLocaleLowerCase('tr')
  const list = q ? options.value.filter((o) => o.label.toLocaleLowerCase('tr').includes(q)) : options.value
  return expanded.value || q || list.length <= 12 ? list : list.slice(0, 12)
})

function isChecked(bit: string) {
  return (value.value & BigInt(bit)) !== 0n
}
function changedBit(bit: string) {
  const b = BigInt(bit)
  return (value.value & b) !== (before.value & b)
}
function toggle(bit: string) {
  emit('update:modelValue', (value.value ^ BigInt(bit)).toString())
}
function setAll(on: boolean) {
  const supported = options.value.filter((o) => !o.unsupported).reduce((m, o) => m | BigInt(o.value), 0n)
  const next = on ? value.value | supported : value.value & ~supported
  emit('update:modelValue', next.toString())
}
function bitIndex(bit: string) {
  return BigInt(bit).toString(2).length - 1
}
</script>

<template>
  <div class="bitmask">
    <div class="toolbar">
      <span class="count">{{ selectedCount }} / {{ options.length }} seçili</span>
      <input
        v-if="options.length > 8"
        v-model="filter"
        class="mini-search"
        type="search"
        placeholder="Seçeneklerde ara…"
      />
      <button type="button" class="link" @click="setAll(true)">Tümünü seç</button>
      <button type="button" class="link" @click="setAll(false)">Temizle</button>
      <button type="button" class="link" @click="showTech = !showTech">
        {{ showTech ? 'Teknik detayı gizle' : 'Teknik detay' }}
      </button>
    </div>

    <p v-if="!options.length" class="empty">Bu ayarın eski ekranda da tanımlı bir seçeneği yok.</p>

    <ul class="options">
      <li
        v-for="o in visible"
        :key="o.value"
        :class="{ checked: isChecked(o.value), changed: changedBit(o.value), unsupported: o.unsupported }"
      >
        <label :title="o.unsupported ? 'Bu seçeneğin bit değeri veritabanı kolonuna sığmıyor; kaydedilemez.' : ''">
          <input
            type="checkbox"
            :checked="isChecked(o.value)"
            :disabled="o.unsupported"
            @change="toggle(o.value)"
          />
          <span class="text">{{ o.label }}</span>
          <span v-if="o.unsupported" class="badge warn">kolona sığmıyor</span>
          <span v-if="showTech" class="bit">bit {{ bitIndex(o.value) }} = {{ o.value }}</span>
        </label>
      </li>
    </ul>

    <button
      v-if="!filter && !highlight && options.length > 12"
      type="button"
      class="link more"
      @click="expanded = !expanded"
    >
      {{ expanded ? 'Daha az göster' : `Tüm seçenekleri göster (${options.length})` }}
    </button>

    <div v-if="showTech" class="tech">
      <div><b>Kolon:</b> {{ def.key }} ({{ def.sqlType }})</div>
      <div><b>Kayıtlı değer:</b> {{ before.toString() }} → <b>yeni:</b> {{ value.toString() }}</div>
      <div v-if="unknownBits !== 0n">
        <b>Ekranda karşılığı olmayan bitler:</b> {{ unknownBits.toString() }} — kaydederken korunur.
      </div>
    </div>
  </div>
</template>

<style scoped>
.toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px 14px;
  margin-bottom: 8px;
  font-size: 13px;
}
.count {
  color: var(--c-muted);
  font-weight: 600;
}
.mini-search {
  flex: 1 1 180px;
  max-width: 260px;
  padding: 5px 9px;
  font-size: 13px;
}
.options {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 4px 10px;
}
.options li label {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 6px 8px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  line-height: 1.35;
}
.options li label:hover {
  background: var(--c-hover);
}
.options li.checked label {
  background: var(--c-accent-soft);
}
.options li.changed label {
  box-shadow: inset 3px 0 0 var(--c-warn);
}
.options li.unsupported label {
  opacity: 0.6;
  cursor: not-allowed;
}
.options input {
  margin-top: 2px;
  accent-color: var(--c-accent);
}
.text {
  flex: 1;
}
.bit {
  font-family: ui-monospace, monospace;
  font-size: 11px;
  color: var(--c-muted);
  white-space: nowrap;
}
.badge.warn {
  font-size: 11px;
  background: var(--c-danger-soft);
  color: var(--c-danger);
  border-radius: 4px;
  padding: 1px 6px;
  white-space: nowrap;
}
.more {
  margin-top: 6px;
}
.tech {
  margin-top: 10px;
  padding: 10px 12px;
  border-radius: 8px;
  background: var(--c-code-bg);
  font-family: ui-monospace, monospace;
  font-size: 12px;
  display: grid;
  gap: 4px;
  word-break: break-all;
}
.empty {
  color: var(--c-muted);
  font-size: 13px;
  margin: 0;
}
</style>
