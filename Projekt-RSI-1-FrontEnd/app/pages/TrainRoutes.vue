<script setup>
import { ref, onMounted } from 'vue'

const trainRoutes = ref([])
const loading = ref(false)
const error = ref(null)

const filters = ref({
  departureCity: '',
  arrivalCity: '',
  departureDay: null
})

async function fetchTrainRoutes() {
  loading.value = true
  error.value = null
  
  let departureDayXml = ''
  if (filters.value.departureDay) {
    departureDayXml = `<tem:departureDate>${filters.value.departureDay}</tem:departureDate>`
  }

  const soapXml = `
    <soapenv:Envelope 
        xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
        xmlns:tem="http://tempuri.org/" 
        xmlns:sec="http://projektrsi.security">
       <soapenv:Body>
          <tem:SearchTrainRoutes>
             <tem:departureCity>${filters.value.departureCity}</tem:departureCity>
             <tem:arrivalCity>${filters.value.arrivalCity}</tem:arrivalCity>
             ${departureDayXml}
          </tem:SearchTrainRoutes>
       </soapenv:Body>
    </soapenv:Envelope>`

  try {
    const response = await fetch('https://localhost:8181/TrainRouteService', {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml;charset=UTF-8',
        'SOAPAction': 'http://tempuri.org/ITrainRouteService/SearchTrainRoutes'
      },
      body: soapXml
    })

    if (!response.ok) throw new Error(`Błąd serwera: ${response.status}`)

    let xmlText = await response.text()

    if (xmlText.includes('<s:Envelope')) {
      const start = xmlText.indexOf('<s:Envelope')
      const end = xmlText.lastIndexOf('</s:Envelope>') + '</s:Envelope>'.length
      xmlText = xmlText.substring(start, end)
    }

    const parser = new DOMParser()
    const xmlDoc = parser.parseFromString(xmlText, 'text/xml')

    const routeNodes = Array.from(xmlDoc.getElementsByTagName('*'))
      .filter(el => el.localName === 'TrainRoute')

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
    </div>

    <!-- Formularz wyszukiwania -->
    <UCard class="mb-6">
      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4 items-end">
        <div>
          <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 block">
            Miasto wyjazdu
          </label>
          <UInput 
            v-model="filters.departureCity"
            placeholder="np. Warszawa"
            @keyup.enter="fetchTrainRoutes"
          />
        </div>

        <div>
          <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 block">
            Miasto przyjazdu
          </label>
          <UInput 
            v-model="filters.arrivalCity"
            placeholder="np. Kraków"
            @keyup.enter="fetchTrainRoutes"
          />
        </div>

        <div>
          <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 block">
            Data wyjazdu
          </label>
          <UInput 
            v-model="filters.departureDay"
            type="date"
            @keyup.enter="fetchTrainRoutes"
          />
        </div>

        <div class="flex gap-2">
          <UButton 
            icon="i-heroicons-magnifying-glass" 
            color="primary"
            :loading="loading"
            @click="fetchTrainRoutes"
            block
          >
            Szukaj
          </UButton>
          <UButton 
            icon="i-heroicons-x-mark" 
            color="gray" 
            variant="soft"
            @click="() => {
              filters.departureCity = ''
              filters.arrivalCity = ''
              filters.departureDay = null
              fetchTrainRoutes()
            }"
          >
            Wyczyść
          </UButton>
        </div>
      </div>
    </UCard>
    
    <div v-if="loading" class="text-center py-10">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin text-4xl text-primary" />
      <p class="mt-4 text-gray-500">Szukanie tras pociągów...</p>
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
        <p class="text-gray-500">Brak tras spełniających kryteria wyszukiwania.</p>
      </div>
    </div>
  </UContainer>
</template>