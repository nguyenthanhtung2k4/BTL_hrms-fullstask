<script setup lang="ts">
import { ref, watch } from 'vue'
import { departmentService } from '../../../services/department.service'
import { useToastStore } from '../../../stores/toast'
import type { Department, CreateDepartmentDto, UpdateDepartmentDto } from '../../../types/hr.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppButton from '../../../components/ui/AppButton.vue'

const props = defineProps<{ edit: Department | null }>()
const emit = defineEmits<{ close: []; saved: [] }>()

const toast = useToastStore()
const saving = ref(false)

const form = ref({ code: '', name: '', description: '', isActive: true })
const errors = ref<Record<string, string>>({})

// Populate form khi edit
watch(
  () => props.edit,
  (dept) => {
    if (dept) {
      form.value = { code: dept.code, name: dept.name, description: dept.description ?? '', isActive: dept.isActive }
    } else {
      form.value = { code: '', name: '', description: '', isActive: true }
    }
    errors.value = {}
  },
  { immediate: true },
)

function validate() {
  errors.value = {}
  if (!form.value.code.trim()) errors.value.code = 'Mã không được để trống'
  if (!form.value.name.trim()) errors.value.name = 'Tên không được để trống'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (props.edit) {
      const dto: UpdateDepartmentDto = { name: form.value.name, description: form.value.description, isActive: form.value.isActive }
      await departmentService.update(props.edit.id, dto)
      toast.success('Cập nhật phòng ban thành công')
    } else {
      const dto: CreateDepartmentDto = { code: form.value.code, name: form.value.name, description: form.value.description }
      await departmentService.create(dto)
      toast.success('Tạo phòng ban thành công')
    }
    emit('saved')
  } catch (err: any) {
    const msg = err?.response?.data?.message ?? 'Lưu thất bại'
    toast.error(msg)
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <AppModal :title="edit ? 'Sửa phòng ban' : 'Thêm phòng ban'" @close="emit('close')">
    <div class="space-y-4">
      <AppInput
        id="dept-code"
        v-model="form.code"
        label="Mã phòng ban"
        placeholder="VD: IT, HR, FINANCE"
        required
        :disabled="!!edit"
        :error="errors.code"
      />
      <AppInput
        id="dept-name"
        v-model="form.name"
        label="Tên phòng ban"
        placeholder="VD: Phòng Công nghệ thông tin"
        required
        :error="errors.name"
      />
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Mô tả</label>
        <textarea
          v-model="form.description"
          rows="3"
          placeholder="Mô tả chức năng phòng ban..."
          class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400"
        />
      </div>
      <label v-if="edit" class="flex items-center gap-2 cursor-pointer">
        <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded accent-emerald-600" />
        <span class="text-sm text-slate-700">Kích hoạt</span>
      </label>
    </div>

    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" @click="save">
        {{ edit ? 'Cập nhật' : 'Tạo mới' }}
      </AppButton>
    </template>
  </AppModal>
</template>
