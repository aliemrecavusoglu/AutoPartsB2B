<template>
    <div class="container d-flex justify-content-center align-items-center" style="min-height: 80vh;">
        <!-- Kart arka planı temaya göre değişir -->
        <div class="card shadow-lg border-0 border-top border-success border-5"
            :class="store.isDarkMode ? 'bg-dark' : 'bg-light'" style="width: 100%; max-width: 450px;">

            <div class="card-body p-5">
                <div class="text-center mb-4">
                    <!-- Başlık metin renkleri temaya göre ayarlandı -->
                    <h2 class="fw-bolder mb-1" :class="store.isDarkMode ? 'text-white' : 'text-dark'">📝 Bayi Kaydı</h2>
                    <p :class="store.isDarkMode ? 'text-light' : 'text-muted'">B2B Portalına Katılın</p>
                </div>

                <form @submit.prevent="handleRegister">
                    <div class="mb-3">
                        <label class="form-label fw-bold text-secondary">Bayi Kodu (Kullanıcı Adı)</label>
                        <!-- Form kutucukları karanlık modda koyu renk olur -->
                        <input v-model="username" type="text" class="form-control form-control-lg"
                            :class="store.isDarkMode ? 'bg-dark text-white border-secondary' : 'border-secondary-subtle'"
                            placeholder="Örn: sivasoto" required>
                    </div>
                    <div class="mb-4">
                        <label class="form-label fw-bold text-secondary">Şifre</label>
                        <input v-model="password" type="password" class="form-control form-control-lg"
                            :class="store.isDarkMode ? 'bg-dark text-white border-secondary' : 'border-secondary-subtle'"
                            placeholder="••••" required>
                    </div>

                    <button type="submit"
                        class="btn btn-success btn-lg w-100 fw-bold text-light text-uppercase shadow-sm mb-3">
                        <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                        Kayıt Ol
                    </button>
                </form>

                <div class="text-center mt-3">
                    <!-- Alt kısımdaki sorunun görünürlüğü temaya göre düzeltildi -->
                    <span :class="store.isDarkMode ? 'text-light' : 'text-muted'">Zaten bayi misiniz? </span>
                    <router-link to="/login" class="text-success fw-bold text-decoration-none">Giriş Yap</router-link>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { useB2bStore } from '../stores/b2bStore'
import Swal from 'sweetalert2'

const store = useB2bStore()
const username = ref('')
const password = ref('')
const loading = ref(false)

// Şifre uzunluğunu kontrol edip kayıt işlemini başlatır
const handleRegister = async () => {
    if (password.value.length < 4) {
        Swal.fire({ icon: 'warning', title: 'Uyarı', text: 'Şifre en az 4 karakter olmalıdır.' });
        return;
    }

    loading.value = true;
    await store.register(username.value, password.value);
    loading.value = false;
}
</script>