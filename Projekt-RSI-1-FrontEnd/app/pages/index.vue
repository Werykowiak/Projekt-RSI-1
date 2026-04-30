<template>
  <div class="space-y-8">
    <UPageHero
      title="Rezerwacja Pociągów"
      description="System do rezerwacji biletów na pociągi i sprawdzania rezerwacji"
    />

    <UPageSection>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold">Dostępne Trasy</h3>
          </template>
          <p class="text-sm text-gray-600 mb-4">Przeglądaj wszystkie dostępne trasy pociągów i dokonaj rezerwacji</p>
          <UButton to="/trainRoutes" block>
            Przejdź do tras
          </UButton>
        </UCard>

        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold">Sprawdź Rezerwację</h3>
          </template>
          <p class="text-sm text-gray-600 mb-4">Wpisz numer rezerwacji aby sprawdzić jej status</p>
          <UButton to="/checkReservation" block>
            Sprawdź rezerwację
          </UButton>
        </UCard>
      </div>
    </UPageSection>
  </div>
</template>
