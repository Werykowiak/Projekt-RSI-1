<script setup>
import { ref, watch } from 'vue'

const route = useRoute()
const reservationId = ref('')
const reservationData = ref(null)
const loading = ref(false)
const downloadLoading = ref(false)
const error = ref(null)

watch(
  () => route.query.id,
  (id) => {
    if (!id) return

    reservationId.value = Array.isArray(id) ? String(id[0]) : String(id)
    findReservation()
  },
  { immediate: true }
)

// 1. Pobieranie danych tekstowych przez metodę GetReservation
async function findReservation() {
  if (!reservationId.value) return
  
  loading.value = true
  reservationData.value = null
  error.value = null

  const soapXml = `
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
       <soapenv:Header/>
       <soapenv:Body>
          <tem:GetReservation>
             <tem:reservationId>${reservationId.value}</tem:reservationId>
          </tem:GetReservation>
       </soapenv:Body>
    </soapenv:Envelope>`

  try {
    const response = await fetch('https://localhost:8181/ReservationService', {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml;charset=UTF-8',
        'SOAPAction': 'http://tempuri.org/IReservationService/GetReservation'
      },
      body: soapXml
    })

    let text = await response.text()

    // Czyścimy MTOM (jeśli Twój serwer go dodaje, jak przy trasach)
    if (text.includes('<s:Envelope')) {
      const start = text.indexOf('<s:Envelope')
      const end = text.lastIndexOf('</s:Envelope>') + '</s:Envelope>'.length
      text = text.substring(start, end)
    }

    const parser = new DOMParser()
    const xmlDoc = parser.parseFromString(text, 'text/xml')

    // Szukamy tagu z wynikiem - localName ignoruje namespace 'a:'
    const resultNode = Array.from(xmlDoc.getElementsByTagName('*'))
      .find(el => el.localName === 'GetReservationResult')

    if (!resultNode || resultNode.getAttribute('i:nil') === 'true' || !resultNode.hasChildNodes()) {
      throw new Error('Nie znaleziono rezerwacji o takim ID.')
    }

    const getVal = (name) => {
      const el = Array.from(resultNode.getElementsByTagName('*')).find(e => e.localName === name)
      return el ? el.textContent : ''
    }

    reservationData.value = {
      id: getVal('id'),
      firstName: getVal('passengerFirstName'),
      lastName: getVal('passengerLastName'),
      email: getVal('passengerEmail'),
      seats: getVal('numberOfSeats'),
      date: getVal('reservationDate')
    }

  } catch (err) {
    error.value = err.message
    console.error(err)
  } finally {
    loading.value = false
  }
}

// 2. Pobieranie pliku PDF przez metodę GenerateReservationPdf
async function downloadPdf() {
  if (!reservationData.value) return
  downloadLoading.value = true

  const soapXml = `
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
       <soapenv:Header/>
       <soapenv:Body>
          <tem:GenerateReservationPdf>
             <tem:reservationId>${reservationData.value.id}</tem:reservationId>
          </tem:GenerateReservationPdf>
       </soapenv:Body>
    </soapenv:Envelope>`

  try {
    const response = await fetch('https://localhost:8181/ReservationService', {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml;charset=UTF-8',
        'SOAPAction': 'http://tempuri.org/IReservationService/GenerateReservationPdf'
      },
      body: soapXml
    })

    if (!response.ok) throw new Error("Błąd serwera: " + response.status)

    //Pobieramy surowe bajty (przez MTOM)
    const buffer = await response.arrayBuffer()
    const bytes = new Uint8Array(buffer)

    //Szukamy sygnatury pliku PDF (%PDF-) w bajtach
    //%PDF-w systemie szesnastkowym: 25 50 44 46 2D
    let startIdx = -1
    for (let i = 0; i < bytes.length - 5; i++) {
      if (bytes[i] === 0x25 && bytes[i+1] === 0x50 && bytes[i+2] === 0x44 && bytes[i+3] === 0x46) {
        startIdx = i
        break
      }
    }

    if (startIdx === -1) {
      throw new Error("W odpowiedzi nie znaleziono strumienia PDF. Sprawdź czy backend poprawnie generuje dokument.")
    }

    //Szukamy końca pliku PDF (%%EOF -> 25 25 45 4F 46)
    let endIdx = -1
    for (let i = bytes.length - 5; i > startIdx; i--) {
      if (bytes[i] === 0x25 && bytes[i+1] === 0x25 && bytes[i+2] === 0x45 && bytes[i+3] === 0x4F && bytes[i+4] === 0x46) {
        endIdx = i + 5
        break
      }
    }

    if (endIdx === -1) {
      //Jeśli nie znaleziono %%EOF bierzemy wszystko od startu do końca paczki 
      endIdx = bytes.length
    }

    //Wycinamy czysty PDF i tworzymy z niego plik do pobrania
    const pdfBlob = new Blob([bytes.slice(startIdx, endIdx)], { type: 'application/pdf' })
    const blobUrl = URL.createObjectURL(pdfBlob)

    const link = document.createElement('a')
    link.href = blobUrl
    link.download = `${reservationData.value.firstName}_${reservationData.value.lastName}_${reservationData.value.id}.pdf`
    document.body.appendChild(link)
    link.click()

    document.body.removeChild(link)
    URL.revokeObjectURL(blobUrl)

  } catch (err) {
    console.error("Błąd wyciągania PDF z MTOM:", err)
    alert("Błąd: " + err.message)
  } finally {
    downloadLoading.value = false
  }
}
</script>

<template>
  <UContainer class="py-10 max-w-2xl">
    <UCard>
      <template #header>
        <h2 class="text-xl font-bold">Wyszukiwarka biletów</h2>
        <p class="text-sm text-gray-500">Podaj numer rezerwacji, aby zobaczyć szczegóły i pobrać bilet PDF.</p>
      </template>

      <div class="flex gap-2">
        <UInput 
          v-model="reservationId" 
          type="number" 
          placeholder="ID Rezerwacji (np. 15)" 
          class="flex-1" 
          @keyup.enter="findReservation"
        />
        <UButton 
          color="primary"
          icon="i-heroicons-magnifying-glass" 
          :loading="loading" 
          @click="findReservation"
        >
          Szukaj
        </UButton>
      </div>

      <div v-if="error" class="mt-4">
        <UAlert color="red" variant="soft" :title="error" icon="i-heroicons-x-circle" />
      </div>

      <div v-if="reservationData" class="mt-8 border-t border-gray-100 dark:border-gray-800 pt-6 space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="space-y-1">
            <span class="text-xs font-medium text-gray-400 uppercase tracking-wider">Pasażer</span>
            <p class="text-lg font-semibold">{{ reservationData.firstName }} {{ reservationData.lastName }}</p>
          </div>
          <div class="space-y-1">
            <span class="text-xs font-medium text-gray-400 uppercase tracking-wider">ID Rezerwacji</span>
            <p class="text-lg font-mono">#{{ reservationData.id }}</p>
          </div>
          <div class="space-y-1">
            <span class="text-xs font-medium text-gray-400 uppercase tracking-wider">Email</span>
            <p>{{ reservationData.email }}</p>
          </div>
          <div class="space-y-1">
            <span class="text-xs font-medium text-gray-400 uppercase tracking-wider">Liczba miejsc</span>
            <p class="font-bold">{{ reservationData.seats }}</p>
          </div>
        </div>

        <div class="bg-primary-50 dark:bg-primary-950/30 p-4 rounded-xl border border-primary-100 dark:border-primary-900 flex flex-col md:flex-row justify-between items-center gap-4">
          <div class="flex items-center gap-3">
            <UIcon name="i-heroicons-document-check" class="text-2xl text-primary" />
            <span class="text-sm font-medium">Twój bilet jest gotowy do pobrania</span>
          </div>
          <UButton 
            color="primary" 
            icon="i-heroicons-cloud-arrow-down" 
            :loading="downloadLoading"
            @click="downloadPdf"
          >
            Pobierz PDF (Bilet)
          </UButton>
        </div>
      </div>
    </UCard>
  </UContainer>
</template>