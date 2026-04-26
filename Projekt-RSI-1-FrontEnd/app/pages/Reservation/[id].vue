<script setup>
import { ref } from 'vue'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const routeId = Number(route.params.id) 
// Pobieramy miasta z URL
const departure = route.query.departure || 'Nieznane'
const arrival = route.query.arrival || 'Nieznane'
// Sklejamy to w jeden string do wyświetlenia na UI
const routeDisplayName = `${departure} - ${arrival}`

const loading = ref(false)

// Zaktualizowałem nazwy pól pod twój model C# (passengerFirstName itd.)
const reservation = ref({
  trainRouteId: routeId,
  passengerFirstName: '',
  passengerLastName: '',
  passengerEmail: '',
  numberOfSeats: 1
})

async function makeReservation() {
  loading.value = true
  const url = 'https://localhost:8181/ReservationService'
  
  // Upewnij się, że tagi XML pasują do tego, jak ustawiłeś to w usłudze SOAP na backendzie!
  const soapXml = `
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
       <soapenv:Header/>
       <soapenv:Body>
          <tem:BuyTicket>
             <tem:trainRouteId>${reservation.value.trainRouteId}</tem:trainRouteId>
             <tem:firstName>${reservation.value.passengerFirstName}</tem:firstName>
             <tem:lastName>${reservation.value.passengerLastName}</tem:lastName>
             <tem:email>${reservation.value.passengerEmail}</tem:email>
             <tem:numberOfSeats>${reservation.value.numberOfSeats}</tem:numberOfSeats>
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

    // Zamiast od razu rzucać ogólny błąd, sprawdzamy treść odpowiedzi z serwera
    if (!response.ok) {
      const errorText = await response.text()
      
      // Szukamy komunikatu, który wyrzucił Twój backend w C#
      if (errorText.includes('Brak wystarczajacej liczby miejsc')) {
        throw new Error('BRAK_MIEJSC')
      } else {
        throw new Error('BŁĄD_SERWERA')
      }
    }

    toast.add({
      title: 'Sukces!',
      description: `Zarezerwowano bilety na trasę ${routeDisplayName}.`,
      color: 'green',
      icon: 'i-heroicons-check-circle'
    })
    
    router.push('/TrainRoutes')
  } catch (error) {
    console.error('Błąd:', error)
    
    // Obsługa konkretnych błędów
    if (error.message === 'BRAK_MIEJSC') {
      toast.add({
        title: 'Brak wolnych miejsc',
        description: 'Niestety, na wybranej trasie nie ma wystarczającej liczby wolnych miejsc.',
        color: 'orange',
        icon: 'i-heroicons-exclamation-triangle'
      })
    } else {
      toast.add({
        title: 'Wystąpił błąd',
        description: 'Nie udało się złożyć rezerwacji z powodu błędu serwera.',
        color: 'red',
        icon: 'i-heroicons-x-circle'
      })
    }
  } finally {
    loading.value = false
  }
}

function goBack() {
  router.push('/TrainRoutes')
}
</script>

<template>
  <UContainer class="py-10 max-w-3xl">
    <UButton 
      color="gray" 
      variant="ghost" 
      icon="i-heroicons-arrow-left" 
      class="mb-4"
      @click="goBack"
    >
      Wróć do listy połączeń
    </UButton>

    <UCard>
      <template #header>
        <div>
          <h2 class="text-xl font-bold text-gray-900 dark:text-white">
            Kup bilet - <span class="text-primary">Dawidzior & Weryk Trains</span>
          </h2>
          <p class="text-sm text-gray-500 mt-1">Trasa: {{ routeDisplayName }} (ID: {{ routeId }})</p>
        </div>
      </template>

      <form @submit.prevent="makeReservation" class="space-y-5 p-2">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <UFormGroup label="Imię" required>
            <UInput v-model="reservation.passengerFirstName" icon="i-heroicons-user" placeholder="Jan" required />
          </UFormGroup>

          <UFormGroup label="Nazwisko" required>
            <UInput v-model="reservation.passengerLastName" icon="i-heroicons-user" placeholder="Kowalski" required />
          </UFormGroup>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <UFormGroup label="Email" required>
            <UInput v-model="reservation.passengerEmail" type="email" icon="i-heroicons-envelope" placeholder="jan@gmail.com" required />
          </UFormGroup>

          <UFormGroup label="Liczba miejsc" required>
            <UInput v-model="reservation.numberOfSeats" type="number" min="1" max="10" icon="i-heroicons-ticket" required />
          </UFormGroup>
        </div>

        <div class="flex justify-end gap-3 mt-6 border-t border-gray-100 dark:border-gray-800 pt-4">
          <UButton color="gray" variant="soft" @click="goBack">
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
  </UContainer>
</template>