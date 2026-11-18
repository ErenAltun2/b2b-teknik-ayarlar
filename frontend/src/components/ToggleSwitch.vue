<script setup lang="ts">
defineProps<{ modelValue: boolean; id?: string; disabled?: boolean }>()
defineEmits<{ 'update:modelValue': [value: boolean] }>()
</script>

<template>
  <button
    :id="id"
    type="button"
    role="switch"
    class="switch"
    :class="{ on: modelValue }"
    :aria-checked="modelValue"
    :disabled="disabled"
    @click="$emit('update:modelValue', !modelValue)"
  >
    <span class="knob" />
  </button>
</template>

<style scoped>
.switch {
  width: 42px;
  height: 24px;
  border-radius: 999px;
  border: none;
  background: var(--c-border-strong);
  position: relative;
  cursor: pointer;
  transition: background 0.15s;
  flex-shrink: 0;
}
.switch.on {
  background: var(--c-accent);
}
.switch:focus-visible {
  outline: 2px solid var(--c-accent);
  outline-offset: 2px;
}
.knob {
  position: absolute;
  top: 3px;
  left: 3px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 1px 2px rgb(0 0 0 / 0.25);
  transition: transform 0.15s;
}
.on .knob {
  transform: translateX(18px);
}
</style>
