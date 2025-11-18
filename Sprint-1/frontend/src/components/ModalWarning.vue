<template>
  <div v-if="visible" class="modal-backdrop">
    <div class="modal-box" role="dialog" aria-modal="true">
      <header class="modal-header">
        <h3>Confirmar eliminación</h3>
      </header>

      <div class="modal-body">
        <p class="confirm-text">
          ¿Está seguro que desea eliminar <strong>{{ itemName }}</strong>?<br />
          <span class="warning">Esta acción es irreversible.</span>
        </p>
      </div>

      <footer class="modal-footer">
        <button class="btn-secondary" @click="cancel" :disabled="submitting">
          Volver
        </button>
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
  name: "ModalWarning",
  props: {
    visible: { type: Boolean, required: true },
    itemName: { type: String,  default: "" },
    submitting: { type: Boolean, default: false }
  },
  methods: {
    cancel() {
      // Emitimos el evento para que el padre cierre el modal
      this.$emit("volver");
    },
    confirm() {
      // Emitimos el evento con el nombre del item
      this.$emit("confirm", this.itemName);
    }
  }
};
</script>


<style scoped>
.modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.45); display:flex; align-items:center; justify-content:center; z-index:1200; }
.modal-box { width:420px; background:#5e666d; color:#eee; border-radius:8px; padding:16px; box-shadow:0 8px 32px rgba(0,0,0,0.6); }
.modal-header h3 { margin:0 0 8px 0; font-size:1.05rem; }
.confirm-text { color:#ffffff; margin-bottom:8px; }
.warning { color:#ffb4b4; font-weight:bold; }
.modal-footer { background:#5e666d; display:flex; justify-content:flex-end; gap:8px; margin-top:12px; }
.btn-primary { background:#d9534f; color:#ffffff; border-radius:6px; padding:8px 12px; border:none; cursor:pointer; }
.btn-secondary { background:#767f88; color:#ffffff; border-radius:6px; padding:8px 12px; border:1px solid rgba(255,255,255,0.06); cursor:pointer; }
.btn-primary[disabled], .btn-secondary[disabled] { opacity:0.6; cursor:not-allowed; }
</style>
