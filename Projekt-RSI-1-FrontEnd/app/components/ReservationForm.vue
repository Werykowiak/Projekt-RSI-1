<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  routeId: {
    type: Number,
    required: true
  }
})

const emit = defineEmits(['close', 'success'])
const toast = useToast()
const loading = ref(false)

const reservation = ref({
  trainRouteId: props.routeId,
  firstName: '',
  lastName: '',
  email: '',
  seats: 1
})

watch(() => props.routeId, (newId) => {
  reservation.value.trainRouteId = newId
})

async function makeReservation() {
  loading.value = true
  const url = 'https://localhost:8181/ReservationService'
  
  const soapXml = `
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
       <soapenv:Header/>
       <soapenv:Body>
          <tem:BuyTicket>
             <tem:trainRouteId>${reservation.value.trainRouteId}</tem:trainRouteId>
             <tem:firstName>${reservation.value.firstName}</tem:firstName>
             <tem:lastName>${reservation.value.lastName}</tem:lastName>
             <tem:email>${reservation.value.email}</tem:email>
             <tem:numberOfSeats>${reservation.value.seats}</tem:numberOfSeats>
          </tem:BuyTicket>
       </soapenv:Body>
    </soapenv:Envelope>`

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml;charset=UTF-8',
        'SOAPAction': 'http://tempuri.org/IReservationService/BuyTicket' 
      },
      body: soapXml
    })

    if (!response.ok) throw new Error('Błąd serwera')

    toast.add({
      title: 'Sukces!',
      description: `Zarezerwowano bilety na trasę ${props.routeName}.`,
      color: 'green',
      icon: 'i-heroicons-check-circle'
    })
    
    emit('success')
  } catch (error) {
    console.error('Błąd:', error)
    toast.add({
      title: 'Wystąpił błąd',
      description: 'Nie udało się złożyć rezerwacji. Sprawdź konsolę.',
      color: 'red',
      icon: 'i-heroicons-x-circle'
    })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <UCard :ui="{ ring: '', divide: 'divide-y divide-gray-100 dark:divide-gray-800' }">
    
    <template #header>
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-xl font-bold text-gray-900 dark:text-white">
            Kup bilet - <span class="text-primary">Dawidzior & Weryk Trains</span>
          </h2>
          <p class="text-sm text-gray-500 mt-1">Trasa: {{ routeName }} (ID: {{ routeId }})</p>
        </div>
        <UButton 
          color="gray" 
          variant="ghost" 
          icon="i-heroicons-x-mark-20-solid" 
          class="-my-1" 
          @click="emit('close')" 
        />
      </div>
    </template>

    <form @submit.prevent="makeReservation" class="space-y-5 p-2">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <UFormGroup label="Imię" required>
          <UInput v-model="reservation.firstName" icon="i-heroicons-user" placeholder="Jan" required />
        </UFormGroup>

        <UFormGroup label="Nazwisko" required>
          <UInput v-model="reservation.lastName" icon="i-heroicons-user" placeholder="Kowalski" required />
        </UFormGroup>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <UFormGroup label="Email" required>
          <UInput v-model="reservation.email" type="email" icon="i-heroicons-envelope" placeholder="jan@gmail.com" required />
        </UFormGroup>

        <UFormGroup label="Liczba miejsc" required>
          <UInput v-model="reservation.seats" type="number" min="1" max="10" icon="i-heroicons-ticket" required />
        </UFormGroup>
      </div>

      <div class="flex justify-end gap-3 mt-6">
        <UButton color="gray" variant="soft" @click="emit('close')">
          Anuluj
        </UButton>
        <UButton 
          type="submit"
          color="primary" 
          :loading="loading"
          icon="i-heroicons-paper-airplane"
        >
          Potwierdź rezerwację
        </UButton>
      </div>
    </form>

  </UCard>
</template>