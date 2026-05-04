<script setup>
import { ref, onMounted } from 'vue'

const trainRoutes = ref([])
const loading = ref(false)
const error = ref(null)

// Edit mode / form state
const editMode = ref(false)
const apiKey = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const form = ref({
  id: null,
  departureCity: '',
  arrivalCity: '',
  departureTime: '',
  arrivalTime: '',
  price: 0,
  availableSeats: 0
})
const saving = ref(false)
const fieldErrors = ref({})

const availableCurrencies = ['PLN', 'EUR', 'USD', 'GBP']
const filters = ref({
  departureCity: '',
  arrivalCity: '',
  departureDay: null,
  currency: 'PLN' 
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
             <tem:targetCurrency>${filters.value.currency}</tem:targetCurrency>
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

function formatForSoap(dtStr) {
  if (!dtStr) return ''
  // datetime-local produces YYYY-MM-DDTHH:MM — add seconds if missing
  if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(dtStr)) return dtStr + ':00'
  return dtStr
}

function parseSoapFault(xmlText) {
  try {
    const parser = new DOMParser()
    const xmlDoc = parser.parseFromString(xmlText, 'text/xml')
    // look for common fault tags
    const fault = Array.from(xmlDoc.getElementsByTagName('*')).find(e => e.localName === 'Fault' || e.localName === 'fault')
    if (fault) {
      const faultstring = Array.from(fault.getElementsByTagName('*')).find(e => e.localName === 'faultstring' || e.localName === 'Reason' || e.localName === 'Text')
      return faultstring ? faultstring.textContent : fault.textContent || 'SOAP Fault'
    }
    // sometimes fault is under Body > Fault
    const faultAlt = Array.from(xmlDoc.getElementsByTagName('*')).find(e => e.localName === 'faultstring' || e.localName === 'faultcode')
    if (faultAlt) return faultAlt.textContent
  } catch (e) {
    // ignore parse errors
  }
  return null
}

function saveApiKey() {
  if (typeof window !== 'undefined' && window.localStorage) {
    localStorage.setItem('X-Api-Key', apiKey.value)
  }
}

function openAdd() {
  isEditing.value = false
  form.value = { id: null, departureCity: '', arrivalCity: '', departureTime: '', arrivalTime: '', price: 0, availableSeats: 0 }
  showModal.value = true
}

function openEdit(route) {
  isEditing.value = true
  form.value = { ...route }
  showModal.value = true
}

function closeModal() {
  showModal.value = false
}

async function submitForm() {
  // clear previous
  fieldErrors.value = {}

  // basic validation
  if (!form.value.departureCity) fieldErrors.value.departureCity = 'Podaj miasto wyjazdu.'
  if (!form.value.arrivalCity) fieldErrors.value.arrivalCity = 'Podaj miasto przyjazdu.'
  if (!form.value.departureTime) fieldErrors.value.departureTime = 'Podaj datę i godzinę wyjazdu.'
  if (form.value.price === null || form.value.price === '' || Number(form.value.price) < 0) fieldErrors.value.price = 'Podaj poprawną cenę.'
  if (form.value.availableSeats === null || form.value.availableSeats === '' || Number(form.value.availableSeats) < 0) fieldErrors.value.availableSeats = 'Podaj liczbę wolnych miejsc.'

  if (Object.keys(fieldErrors.value).length > 0) return

  saving.value = true
  try {
    if (isEditing.value) {
      await editTrainRouteSoap(form.value)
    } else {
      await addTrainRouteSoap(form.value)
    }
    closeModal()
    await fetchTrainRoutes()
  } catch (e) {
    console.error(e)
    error.value = e?.message || 'Błąd przy zapisywaniu połączenia.'
  } finally {
    saving.value = false
  }
}

async function addTrainRouteSoap(payload) {
  const soapXml = `
    <soapenv:Envelope 
xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
xmlns:tem="http://tempuri.org/"
xmlns:proj="http://schemas.datacontract.org/2004/07/Projekt_RSI_1_BackEnd.Models"
xmlns:sec="http://projektrsi.security">
       <soapenv:Header>
         <sec:X-Api-Key>${apiKey.value}</sec:X-Api-Key>
       </soapenv:Header>
       <soapenv:Body>
          <tem:AddTrainRoute>
            <tem:trainRoute>
            <proj:arrivalCity>${payload.arrivalCity}</proj:arrivalCity>
            <proj:arrivalTime>${formatForSoap(payload.arrivalTime) || ''}</proj:arrivalTime>
            <proj:availableSeats>${payload.availableSeats}</proj:availableSeats>
             <proj:departureCity>${payload.departureCity}</proj:departureCity>
             
             <proj:departureTime>${formatForSoap(payload.departureTime)}</proj:departureTime>
             
             <proj:price>${payload.price}</proj:price>
             
            </tem:trainRoute>
          </tem:AddTrainRoute>
       </soapenv:Body>
    </soapenv:Envelope>`

  const res = await fetch('https://localhost:8181/TrainRouteService', {
    method: 'POST',
    headers: {
      'Content-Type': 'text/xml;charset=UTF-8',
      'SOAPAction': 'http://tempuri.org/ITrainRouteService/AddTrainRoute'
    },
    body: soapXml
  })

  const text = await res.text()
  const fault = parseSoapFault(text)
  if (!res.ok || fault) {
    throw new Error(fault || `Add failed: ${res.status}`)
  }
  return text
}

async function editTrainRouteSoap(payload) {
  const soapXml = `
    <soapenv:Envelope 
xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
xmlns:tem="http://tempuri.org/"
xmlns:proj="http://schemas.datacontract.org/2004/07/Projekt_RSI_1_BackEnd.Models"
xmlns:sec="http://projektrsi.security">
       <soapenv:Header>
          <sec:X-Api-Key>${apiKey.value}</sec:X-Api-Key>
       </soapenv:Header>
       <soapenv:Body>
       <tem:EditTrainRoute>
          <tem:trainRoute>
            <proj:arrivalCity>${payload.arrivalCity}</proj:arrivalCity>
            <proj:arrivalTime>${formatForSoap(payload.arrivalTime) || ''}</proj:arrivalTime>
            <proj:availableSeats>${payload.availableSeats}</proj:availableSeats>
             <proj:departureCity>${payload.departureCity}</proj:departureCity>
             
             <proj:departureTime>${formatForSoap(payload.departureTime)}</proj:departureTime>
             <proj:id>${payload.id}</proj:id>
             <proj:price>${payload.price}</proj:price>
          </tem:trainRoute>
       </tem:EditTrainRoute>
       </soapenv:Body>
    </soapenv:Envelope>`

  const res = await fetch('https://localhost:8181/TrainRouteService', {
    method: 'POST',
    headers: {
      'Content-Type': 'text/xml;charset=UTF-8',
      'SOAPAction': 'http://tempuri.org/ITrainRouteService/EditTrainRoute'
    },
    body: soapXml
  })

  const text = await res.text()
  const fault = parseSoapFault(text)
  if (!res.ok || fault) {
    throw new Error(fault || `Edit failed: ${res.status}`)
  }
  return text
}

async function deleteTrainRouteSoap(id) {
  const soapXml = `
    <soapenv:Envelope 
        xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
        xmlns:tem="http://tempuri.org/" 
        xmlns:sec="http://projektrsi.security">
       <soapenv:Header>
         <sec:X-Api-Key>${apiKey.value}</sec:X-Api-Key>
       </soapenv:Header>
       <soapenv:Body>
          <tem:DeleteTrainRoute>
             <tem:id>${id}</tem:id>
          </tem:DeleteTrainRoute>
       </soapenv:Body>
    </soapenv:Envelope>`

  const res = await fetch('https://localhost:8181/TrainRouteService', {
    method: 'POST',
    headers: {
      'Content-Type': 'text/xml;charset=UTF-8',
      'SOAPAction': 'http://tempuri.org/ITrainRouteService/DeleteTrainRoute'
    },
    body: soapXml
  })

  const text = await res.text()
  const fault = parseSoapFault(text)
  if (!res.ok || fault) {
    throw new Error(fault || `Delete failed: ${res.status}`)
  }
  return text
}

async function confirmAndDelete(route) {
  if (!confirm(`Czy na pewno usunąć połączenie ${route.departureCity} → ${route.arrivalCity}?`)) return
  try {
    await deleteTrainRouteSoap(route.id)
    await fetchTrainRoutes()
  } catch (e) {
    console.error(e)
    error.value = 'Błąd podczas usuwania połączenia.'
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
  if (typeof window !== 'undefined' && window.localStorage) {
    apiKey.value = localStorage.getItem('X-Api-Key') || ''
  }
  fetchTrainRoutes()
})
</script>

<template>
  <UContainer class="py-10">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Dostępne połączenia</h1>
    </div>

    <div class="flex items-center gap-4 mb-4">
      <UInput v-model="apiKey" type="password" placeholder="Klucz API (X-Api-Key)" class="w-72" />
      <UButton size="sm" color="primary" @click="saveApiKey">Zapisz klucz</UButton>
      <div class="ml-auto flex items-center gap-2">
         <UCheckbox indicator="end" variant="card" v-model="editMode" label="Tryb edycji" />
        <UButton size="sm" color="success" @click="openAdd" :disabled="!editMode">Dodaj połączenie</UButton>
      </div>
    </div>

    <UCard class="mb-6">
      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4 items-end">
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

        <div>
          <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 block">
            Waluta
          </label>
          <select 
            v-model="filters.currency" 
            @change="fetchTrainRoutes"
            class="block w-full rounded-md border-0 py-1.5 text-gray-900 dark:text-white dark:bg-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 dark:ring-gray-700 focus:ring-2 focus:ring-inset focus:ring-primary-500 sm:text-sm sm:leading-6 px-3"
          >
            <option value="PLN">PLN</option>
            <option value="EUR">EUR</option>
            <option value="USD">USD</option>
            <option value="GBP">GBP</option>
          </select>
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
              filters.currency = 'PLN' 
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
                {{ route.price.toFixed(2) }} {{ filters.currency }}
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

          <div v-if="editMode" class="flex gap-2">
            <UButton size="sm" color="warning" variant="soft" @click.prevent="openEdit(route)">Edytuj</UButton>
            <UButton size="sm" color="danger" variant="soft" @click.prevent="confirmAndDelete(route)">Usuń</UButton>
          </div>
        </div>
      </UCard>
      
      <div v-if="trainRoutes.length === 0" class="text-center py-20 border-2 border-dashed border-gray-200 dark:border-gray-800 rounded-xl">
        <UIcon name="i-heroicons-magnifying-glass" class="text-5xl text-gray-300 mb-2" />
        <p class="text-gray-500">Brak tras spełniających kryteria wyszukiwania.</p>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 flex items-center justify-center z-50">
      <div class="absolute inset-0 bg-black opacity-40" @click="closeModal" aria-hidden="true"></div>
      <UCard class="z-50 w-full max-w-2xl p-0 overflow-hidden" role="dialog" aria-modal="true">
        <div class="flex items-center justify-between px-6 py-4 border-b">
          <div>
            <h3 class="text-lg font-semibold">{{ isEditing ? 'Edytuj połączenie' : 'Dodaj połączenie' }}</h3>
            <p class="text-sm text-gray-500">Uzupełnij pola i kliknij Zapisz. Pola oznaczone * są wymagane.</p>
          </div>
          <button class="text-gray-500 hover:text-gray-700" @click="closeModal" aria-label="Zamknij">
            <UIcon name="i-heroicons-x-mark" />
          </button>
        </div>

        <div class="p-6">
          <form @submit.prevent="submitForm" class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Miasto wyjazdu *</label>
              <UInput v-model="form.departureCity" placeholder="np. Warszawa" />
              <p v-if="fieldErrors.departureCity" class="text-xs text-red-600 mt-1">{{ fieldErrors.departureCity }}</p>
            </div>

            <div>
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Miasto przyjazdu *</label>
              <UInput v-model="form.arrivalCity" placeholder="np. Kraków" />
              <p v-if="fieldErrors.arrivalCity" class="text-xs text-red-600 mt-1">{{ fieldErrors.arrivalCity }}</p>
            </div>

            <div>
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Data i godzina (wyjazd) *</label>
              <UInput v-model="form.departureTime" type="datetime-local" />
              <p v-if="fieldErrors.departureTime" class="text-xs text-red-600 mt-1">{{ fieldErrors.departureTime }}</p>
            </div>

            <div>
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Data i godzina (przyjazd)</label>
              <UInput v-model="form.arrivalTime" type="datetime-local" />
              <p v-if="fieldErrors.arrivalTime" class="text-xs text-red-600 mt-1">{{ fieldErrors.arrivalTime }}</p>
            </div>

            <div>
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Cena (PLN) *</label>
              <UInput v-model.number="form.price" type="number" placeholder="np. 99.99" />
              <p v-if="fieldErrors.price" class="text-xs text-red-600 mt-1">{{ fieldErrors.price }}</p>
            </div>

            <div>
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Wolne miejsca *</label>
              <UInput v-model.number="form.availableSeats" type="number" placeholder="np. 50" />
              <p v-if="fieldErrors.availableSeats" class="text-xs text-red-600 mt-1">{{ fieldErrors.availableSeats }}</p>
            </div>

          </form>

          <div v-if="error" class="mt-4">
            <UAlert color="red" variant="soft" :title="error" />
          </div>

          <div class="mt-6 flex justify-end gap-2">
            <UButton color="gray" variant="soft" @click="closeModal">Anuluj</UButton>
            <UButton :loading="saving" color="primary" @click="submitForm">{{ saving ? 'Zapisywanie...' : 'Zapisz' }}</UButton>
          </div>
        </div>
      </UCard>
    </div>
  </UContainer>
</template>