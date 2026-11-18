<script setup lang="ts">
import { computed } from 'vue'
import type { SettingDefinition, SettingValues } from '../api/types'
import { dependencyMet, isOn } from '../composables/useSettings'
import BitmaskField from './BitmaskField.vue'
import ToggleSwitch from './ToggleSwitch.vue'

const props = defineProps<{
  def: SettingDefinition
  draft: SettingValues
  original: string | null
  dirty: boolean
  errors?: string[]
  dependencyLabel?: string
  highlight?: string
}>()
const emit = defineEmits<{ goTo: [key: string] }>()

const value = computed({
  get: () => props.draft[props.def.key] ?? null,
  set: (v: string | null) => {
    // eslint-disable-next-line vue/no-mutating-props -- draft is a shared reactive store
    props.draft[props.def.key] = v
  },
})

const inputId = computed(() => `s-${props.def.key}`)
const depOk = computed(() => dependencyMet(props.def, props.draft))

/** Keep a stored value visible even if it is not one of the known options. */
const selectOptions = computed(() => {
  const opts = (props.def.options ?? []).filter((o) => !o.unsupported)
  const v = value.value
  if (v != null && !opts.some((o) => o.value === v))
    return [...opts, { value: v, label: `Kayıtlı değer: ${v} (tanımsız)` }]
  return opts
})

const textLimit = computed(() => (props.def.maxLength > 0 && props.def.sqlType === 'nvarchar' ? props.def.maxLength : undefined))

function revert() {
  value.value = props.original
}
</script>

<template>
  <article :id="`setting-${def.key}`" class="setting" :class="{ dirty, invalid: errors?.length, muted: !depOk }">
    <header>
      <div class="title">
        <label :for="def.control === 'bitmask' ? undefined : inputId">{{ def.label }}</label>
        <span v-if="dirty" class="dot" title="Kaydedilmemiş değişiklik" />
      </div>

      <div v-if="def.control === 'switch'" class="inline-control">
        <span class="state">{{ isOn(value) ? 'Açık' : 'Kapalı' }}</span>
        <ToggleSwitch
          :id="inputId"
          :model-value="isOn(value)"
          @update:model-value="(on) => (value = on ? '1' : '0')"
        />
      </div>
    </header>

    <p v-if="def.description" class="help">{{ def.description }}</p>

    <p v-if="def.dependsOn && !depOk" class="dependency">
      ⚠ {{ def.dependsOn.message }}
      <button type="button" class="link" @click="emit('goTo', def.dependsOn.key)">
        “{{ dependencyLabel ?? def.dependsOn.key }}” ayarına git
      </button>
    </p>

    <div class="control">
      <select v-if="def.control === 'select'" :id="inputId" v-model="value">
        <option v-if="value == null" :value="null" disabled>Seçilmemiş</option>
        <option v-for="o in selectOptions" :key="o.value" :value="o.value">{{ o.label }}</option>
      </select>

      <input
        v-else-if="def.control === 'number'"
        :id="inputId"
        v-model="value"
        type="number"
        inputmode="decimal"
        :min="def.min"
        class="narrow"
      />

      <textarea
        v-else-if="def.control === 'textarea'"
        :id="inputId"
        v-model="value"
        rows="3"
        :maxlength="textLimit"
        spellcheck="false"
      />

      <div v-else-if="def.control === 'color'" class="color-row">
        <input
          type="color"
          :value="value && /^#[0-9a-f]{6}$/i.test(value) ? value : '#000000'"
          aria-label="Renk seç"
          @input="value = ($event.target as HTMLInputElement).value"
        />
        <input :id="inputId" v-model="value" type="text" class="narrow mono" placeholder="#RRGGBB" maxlength="7" />
      </div>

      <div v-else-if="def.control === 'image'" class="image-row">
        <input :id="inputId" v-model="value" type="text" :maxlength="textLimit" placeholder="dosya-adi.png" />
        <small>Dosya adı. Görsel yükleme mevcut ekranın dosya yöneticisi üzerinden yapılır.</small>
      </div>

      <BitmaskField
        v-else-if="def.control === 'bitmask'"
        v-model="value"
        :def="def"
        :original="original"
        :highlight="highlight"
      />

      <input
        v-else-if="def.control === 'text'"
        :id="inputId"
        v-model="value"
        type="text"
        :maxlength="textLimit"
        :placeholder="def.placeholder"
      />
    </div>

    <ul v-if="errors?.length" class="errors">
      <li v-for="e in errors" :key="e">{{ e }}</li>
    </ul>

    <footer>
      <code class="column" :title="`DealerSettings.${def.key} (${def.sqlType})`">{{ def.key }}</code>
      <button v-if="dirty" type="button" class="link" @click="revert">Değişikliği geri al</button>
    </footer>
  </article>
</template>

<style scoped>
.setting {
  background: var(--c-surface);
  border: 1px solid var(--c-border);
  border-radius: 12px;
  padding: 16px 18px 12px;
  display: grid;
  gap: 8px;
  scroll-margin-top: 90px;
  transition: border-color 0.15s, box-shadow 0.15s;
}
.setting.dirty {
  border-color: var(--c-warn);
}
.setting.invalid {
  border-color: var(--c-danger);
}
.setting.flash {
  box-shadow: 0 0 0 3px var(--c-accent-soft);
}
.setting.muted .control {
  opacity: 0.7;
}
header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
}
.title {
  display: flex;
  align-items: center;
  gap: 8px;
}
.title label {
  font-weight: 600;
  font-size: 15px;
}
.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--c-warn);
}
.inline-control {
  display: flex;
  align-items: center;
  gap: 10px;
}
.state {
  font-size: 13px;
  color: var(--c-muted);
}
.help {
  margin: 0;
  color: var(--c-muted);
  font-size: 13px;
  line-height: 1.45;
}
.dependency {
  margin: 0;
  font-size: 13px;
  background: var(--c-warn-soft);
  border-radius: 8px;
  padding: 8px 10px;
}
.control select,
.control input[type='text'],
.control textarea {
  width: 100%;
  max-width: 560px;
}
.narrow {
  max-width: 180px;
}
.mono {
  font-family: ui-monospace, monospace;
}
.color-row {
  display: flex;
  gap: 10px;
  align-items: center;
}
.color-row input[type='color'] {
  width: 42px;
  height: 36px;
  padding: 2px;
  border-radius: 8px;
}
.image-row {
  display: grid;
  gap: 4px;
}
.image-row small {
  color: var(--c-muted);
}
.errors {
  margin: 0;
  padding: 8px 10px 8px 26px;
  background: var(--c-danger-soft);
  color: var(--c-danger);
  border-radius: 8px;
  font-size: 13px;
}
footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  min-height: 20px;
}
.column {
  font-size: 11px;
  color: var(--c-muted);
  background: var(--c-code-bg);
  padding: 2px 6px;
  border-radius: 4px;
}
</style>
