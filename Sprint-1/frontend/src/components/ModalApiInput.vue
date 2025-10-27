<template>
  <div v-if="visible" class="modal-backdrop">
    <div class="modal-box" role="dialog" aria-modal="true">
      <header class="modal-header">
        <h3>{{ title }}</h3>
      </header>

      <div class="modal-body">
        <p class="instructions" v-if="instructions">{{ instructions }}</p>

        <!-- Confirm-only: mensaje y botón -->
        <div v-if="mode === 'confirm'">
          <p class="confirm-text">{{ confirmText || 'Esta decisión es final. ¿Continuar?' }}</p>
        </div>

        <!-- Dependientes -->
        <div v-else-if="mode === 'dependientes'" class="field">
          <label for="dependientes">Cantidad de dependientes</label>
          <input
            id="dependientes"
            type="number"
            min="0"
            v-model.number="localValue"
            :disabled="submitting"
            @keydown.enter.prevent="confirm"
          />
          <div v-if="error" class="error">{{ error }}</div>
        </div>

        <!-- Tipo de pensión -->
        <div v-else-if="mode === 'tipoPension'" class="field">
          <label for="tipoPension">Tipo de pensión (A, B o C)</label>
          <input
            id="tipoPension"
            type="text"
            maxlength="1"
            v-model="localValue"
            :disabled="submitting"
            @keydown.enter.prevent="confirm"
          />
          <div v-if="error" class="error">{{ error }}</div>
        </div>

        <!-- Fallback: vacio -->
        <div v-else>
          <p> Sin datos adicionales requeridos.</p>
        </div>
      </div>

      <footer class="modal-footer">
        <button class="btn-secondary" @click="cancel" :disabled="submitting">Cancelar</button>
        <button class="btn-primary" @click="confirm" :disabled="submitting">
          <span v-if="submitting">Enviando…</span>
          <span v-else>Confirmar</span>
        </button>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: "ModalApiInput",
  props: {
    visible: { type: Boolean, required: true },
    mode: { type: String, required: true }, // 'confirm' | 'dependientes' | 'tipoPension'
    title: { type: String, default: "Datos adicionales" },
    instructions: { type: String, default: "" },
    confirmText: { type: String, default: null },
    initialValue: { type: [String, Number, null], default: null },
    submitting: { type: Boolean, default: false }
  },
  data() {
    return {
      localValue: this.initialValue,
      error: ""
    };
  },
  watch: {
    initialValue(v) { this.localValue = v; },
    visible(v) {
      if (v) { this.error = ""; this.localValue = this.initialValue; }
    }
  },
  methods: {
    cancel() {
      this.error = "";
      this.$emit("cancel");
    },
    confirm() {
      this.error = "";
      if (this.mode === "confirm") {
        this.$emit("confirm", null);
        return;
      }
      if (this.mode === "dependientes") {
        const n = Number(this.localValue);
        if (!Number.isFinite(n) || n < 0 || !Number.isInteger(n)) {
          this.error = "Ingrese un número entero válido (0 o mayor).";
          return;
        }
        this.$emit("confirm", { dependientes: n });
        return;
      }
      if (this.mode === "tipoPension") {
        const raw = (this.localValue || "").toString().trim().toUpperCase();
        if (!/^[ABC]$/.test(raw)) {
          this.error = "Ingrese una letra válida: A, B o C.";
          return;
        }
        this.$emit("confirm", { tipoPension: raw });
        return;
      }
      this.$emit("confirm", null);
    }
  }
};
</script>

<style scoped>
.modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.45); display:flex; align-items:center; justify-content:center; z-index:1200; }
.modal-box { width:420px; background:#0f172a; color:#eee; border-radius:8px; padding:16px; box-shadow:0 8px 32px rgba(0,0,0,0.6); }
.modal-header h3 { margin:0 0 8px 0; font-size:1.05rem; }
.instructions, .confirm-text { color:#cbd5e1; margin-bottom:8px; }
.field label { display:block; font-size:0.9rem; margin-bottom:6px; color:#cbd5e1; }
.field input { width:100%; padding:8px 10px; border-radius:6px; border:1px solid rgba(255,255,255,0.08); background: rgba(255,255,255,0.02); color:#fff; }
.error { color:#ffb4b4; margin-top:6px; font-size:0.85rem; }
.modal-footer { display:flex; justify-content:flex-end; gap:8px; margin-top:12px; }
.btn-primary { background:#1fb9b4; color:#042027; border-radius:6px; padding:8px 12px; border:none; cursor:pointer; }
.btn-secondary { background:transparent; color:#cbd5e1; border-radius:6px; padding:8px 12px; border:1px solid rgba(255,255,255,0.06); cursor:pointer; }
.btn-primary[disabled], .btn-secondary[disabled] { opacity:0.6; cursor:not-allowed; }
</style>
