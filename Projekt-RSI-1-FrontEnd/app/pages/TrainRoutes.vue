<script setup>
import { ref, onMounted } from 'vue'

const trainRoutes = ref([])
const loading = ref(false)
const error = ref(null)

async function fetchTrainRoutes() {
  loading.value = true
  error.value = null

  const soapXml = `
    <soapenv:Envelope 
        xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
        xmlns:tem="http://tempuri.org/" 
        xmlns:sec="http://projektrsi.security">
       <soapenv:Header>
          <sec:X-Api-Key>GIGATAJNYKLUCZDOAPI</sec:X-Api-Key>
       </soapenv:Header>
       <soapenv:Body>
          <tem:GetAllTrainRoutes/>
       </soapenv:Body>
    </soapenv:Envelope>`

  try {
    const response = await fetch('https://localhost:8181/TrainRouteService', {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml;charset=UTF-8',
        'SOAPAction': 'http://tempuri.org/ITrainRouteService/GetAllTrainRoutes'
      },
      body: soapXml
    })

    if (!response.ok) throw new Error(`Błąd serwera: ${response.status}`)

    let xmlText = await response.text()

    // --- POPRAWKA MTOM: Wycinamy czysty XML z paczki binarnej ---
    if (xmlText.includes('<s:Envelope')) {
      const start = xmlText.indexOf('<s:Envelope')
      const end = xmlText.lastIndexOf('</s:Envelope>') + '</s:Envelope>'.length
      xmlText = xmlText.substring(start, end)
    }

    const parser = new DOMParser()
    const xmlDoc = parser.parseFromString(xmlText, 'text/xml')

    // 1. Znajdujemy węzły <a:TrainRoute> (ignorujemy namespace przez localName)
    const routeNodes = Array.from(xmlDoc.getElementsByTagName('*'))
      .filter(el => el.localName === 'TrainRoute')

    // 2. Mapujemy węzły na obiekty JS
    trainRoutes.value = routeNodes.map(node => {
      const getValue = (propName) => {
        // Używamy getElementsByTagName, żeby znaleźć pola typu <a:departureCity>
        const elements = node.getElementsByTagName('*')
        const el = Array.from(elements).find(e => e.localName === propName)
        return el ? el.textContent : ''
      }

      return {
        id: Number(getValue('id')),
        departureCity: getValue('departureCity'),
        arrivalCity: getValue('arrivalCity'),
        departureTime: getValue('departureTime'),
        price: Number(getValue('price')),
        availableSeats: Number(getValue('availableSeats'))
      }
    }).filter(route => route.id > 0)

  } catch (err) {
    console.error('Błąd pobierania danych:', err)
    error.value = 'Nie udało się pobrać tras pociągów.'
  } finally {
    loading.value = false
  }
}

function formatDate(dateString) {
  if (!dateString || dateString === '') return 'Brak danych'
  try {
    const date = new Date(dateString)
    return date.toLocaleString('pl-PL', { 
        day: '2-digit', 
        month: '2-digit', 
        year: 'numeric',
        hour: '2-digit', 
        minute: '2-digit' 
    })
  } catch (e) {
    return dateString
  }
}

onMounted(() => {
  fetchTrainRoutes()
})
</script>

<template>
  <UContainer class="py-10">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Dostępne połączenia</h1>
      <UButton 
        icon="i-heroicons-arrow-path" 
        color="gray" 
        variant="ghost" 
        :loading="loading" 
        @click="fetchTrainRoutes"
      >
        Odśwież
      </UButton>
    </div>
    
    <div v-if="loading" class="text-center py-10">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin text-4xl text-primary" />
      <p class="mt-4 text-gray-500">Pobieranie rozkładu jazdy z bazy...</p>
    </div>

    <UAlert 
      v-else-if="error" 
      color="red" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle" 
      :title="error" 
    />

    <div v-else class="space-y-4">
      <UCard v-for="route in trainRoutes" :key="route.id" class="hover:ring-2 hover:ring-primary transition-all">
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div class="flex-1">
            <div class="flex items-center gap-2 mb-1">
              <span class="text-xl font-bold text-gray-900 dark:text-white">{{ route.departureCity }}</span>
              <UIcon name="i-heroicons-arrow-long-right" class="text-primary text-xl" />
              <span class="text-xl font-bold text-gray-900 dark:text-white">{{ route.arrivalCity }}</span>
            </div>
            
            <div class="flex flex-wrap gap-x-4 gap-y-1 text-sm text-gray-500">
              <span class="flex items-center gap-1">
                <UIcon name="i-heroicons-calendar" />
                {{ formatDate(route.departureTime) }}
              </span>
              <span class="flex items-center gap-1 font-semibold text-primary">
                <UIcon name="i-heroicons-banknotes" />
                {{ route.price }} zł
              </span>
              <span class="flex items-center gap-1">
                <UIcon name="i-heroicons-users" />
                Wolne miejsca: {{ route.availableSeats }}
              </span>
            </div>
          </div>
          
          <UButton 
            size="lg"
            color="primary" 
            icon="i-heroicons-ticket"
            :disabled="route.availableSeats <= 0"
            :to="{ 
              path: `/Reservation/${route.id}`, 
              query: { 
                departure: route.departureCity, 
                arrival: route.arrivalCity 
              } 
            }"
          >
            {{ route.availableSeats > 0 ? 'Rezerwuj bilet' : 'Brak miejsc' }}
          </UButton>
        </div>
      </UCard>
      
      <div v-if="trainRoutes.length === 0" class="text-center py-20 border-2 border-dashed border-gray-200 dark:border-gray-800 rounded-xl">
        <UIcon name="i-heroicons-magnifying-glass" class="text-5xl text-gray-300 mb-2" />
        <p class="text-gray-500">Brak dostępnych tras w bazie danych pociągów.</p>
      </div>
    </div>
  </UContainer>
</template>