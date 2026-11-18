<script setup lang="ts">
import { onMounted, ref } from 'vue'
import type { ChangeSummary } from '../composables/useSettings'

defineProps<{ changes: ChangeSummary[]; saving: boolean }>()
const emit = defineEmits<{ confirm: []; cancel: [] }>()

const confirmButton = ref<HTMLButtonElement>()
onMounted(() => confirmButton.value?.focus())
</script>

<template>
  <div class="backdrop" @click.self="emit('cancel')" @keydown.esc="emit('cancel')">
    <div class="dialog" role="dialog" aria-modal="true" aria-labelledby="summary-title">
      <h2 id="summary-title">{{ changes.length }} ayar değiştirilecek</h2>
      <p class="hint">Kaydetmeden önce değişiklikleri kontrol edin. Kaydedilen ayarlar bayi portalında hemen geçerli olur.</p>

      <ul class="list">
        <li v-for="c in changes" :key="c.key">
          <div class="name">{{ c.label }}</div>

          <div v-if="c.added || c.removed" class="bits">
            <div v-for="a in c.added" :key="'+' + a" class="added">+ {{ a }}</div>
            <div v-for="r in c.removed" :key="'-' + r" class="removed">− {{ r }}</div>
          </div>

          <div v-else class="diff">
            <span class="before">{{ c.before }}</span>
            <span class="arrow">→</span>
            <span class="after">{{ c.after }}</span>
          </div>
        </li>
      </ul>

      <div class="actions">
        <button type="button" class="btn ghost" :disabled="saving" @click="emit('cancel')">Vazgeç</button>
        <button ref="confirmButton" type="button" class="btn primary" :disabled="saving" @click="emit('confirm')">
          {{ saving ? 'Kaydediliyor…' : 'Onayla ve Kaydet' }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.backdrop {
  position: fixed;
  inset: 0;
  z-index: 50;
  background: rgb(15 20 30 / 0.45);
  display: grid;
  place-items: center;
  padding: 20px;
}
.dialog {
  background: var(--c-surface);
  border-radius: 14px;
  width: min(620px, 100%);
  max-height: 85vh;
  display: flex;
  flex-direction: column;
  padding: 22px 24px 18px;
  box-shadow: 0 20px 50px rgb(0 0 0 / 0.25);
}
h2 {
  margin: 0 0 4px;
  font-size: 19px;
}
.hint {
  margin: 0 0 14px;
  color: var(--c-muted);
  font-size: 13px;
}
.list {
  list-style: none;
  margin: 0;
  padding: 0;
  overflow-y: auto;
  border-top: 1px solid var(--c-border);
}
.list li {
  display: grid;
  grid-template-columns: minmax(160px, 38%) 1fr;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid var(--c-border);
  font-size: 14px;
}
.name {
  font-weight: 600;
}
.diff {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: baseline;
}
.before {
  color: var(--c-muted);
  text-decoration: line-through;
}
.arrow {
  color: var(--c-muted);
}
.after {
  font-weight: 600;
}
.bits {
  display: grid;
  gap: 2px;
}
.added {
  color: var(--c-success);
}
.removed {
  color: var(--c-danger);
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 16px;
}
@media (max-width: 560px) {
  .list li {
    grid-template-columns: 1fr;
    gap: 4px;
  }
}
</style>
