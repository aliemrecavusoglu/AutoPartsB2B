<template>
    <div class="container d-flex justify-content-center align-items-center" style="min-height: 80vh;">
        <!-- Kart arka planı temaya göre değişir -->
        <div class="card shadow-lg border-0 border-top border-warning border-5"
            :class="store.isDarkMode ? 'bg-dark' : 'bg-light'" style="width: 100%; max-width: 450px;">
            <div class="card-body p-5">
                <div class="text-center mb-4">
                    <!-- Başlık metin renkleri temaya göre ayarlandı -->
                    <h2 class="fw-bolder mb-1" :class="store.isDarkMode ? 'text-white' : 'text-dark'">🛠️ B2B Portal
                    </h2>
                    <p :class="store.isDarkMode ? 'text-light' : 'text-muted'">Kayseri Oto Yedek Parça</p>
                </div>

                <form @submit.prevent="handleLogin">
                    <div class="mb-3">
                        <label class="form-label fw-bold text-secondary">Bayi Kodu (Kullanıcı Adı)</label>
                        <!-- Form kutucukları karanlık modda koyu renk olur -->
                        <input v-model="username" type="text" class="form-control form-control-lg"
                            :class="store.isDarkMode ? 'bg-dark text-white border-secondary' : 'bg-light border-secondary-subtle'"
                            placeholder="Örn: kayseri" required>
                    </div>
                    <div class="mb-4">
                        <label class="form-label fw-bold text-secondary">Şifre</label>
                        <!-- Form kutucukları karanlık modda koyu renk olur -->
                        <input v-model="password" type="password" class="form-control form-control-lg"
                            :class="store.isDarkMode ? 'bg-dark text-white border-secondary' : 'bg-light border-secondary-subtle'"
                            placeholder="••••" required>
                    </div>

                    <button type="submit"
                        class="btn btn-warning btn-lg w-100 fw-bold text-dark text-uppercase shadow-sm mb-3">
                        <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"
                            aria-hidden="true"></span>
                        Giriş Yap
                    </button>
                </form>
                <div class="text-center mt-3">
                    <!-- Alt kısımdaki sorunun görünürlüğü temaya göre düzeltildi -->
                    <span :class="store.isDarkMode ? 'text-light' : 'text-muted'">Sistemde kaydınız yok mu? </span>
                    <router-link to="/register" class="text-warning fw-bold text-decoration-none">Bayi Başvurusu
                        Yap</router-link>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { useB2bStore } from '../stores/b2bStore'

const store = useB2bStore()
const username = ref('')
const password = ref('')
const loading = ref(false)

const handleLogin = async () => {
    loading.value = true;
    await store.login(username.value, password.value);
    loading.value = false;
}
</script>