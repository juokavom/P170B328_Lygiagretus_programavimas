package main

import (
	DM "./DataMonitor"
	IT "./Item"
	"sync"
)

func main() {
	mainData := make(chan IT.Item)
	dataWorker := make(chan IT.Item)
	workerResult := make(chan IT.ItemWithResult)
	resultMain := make(chan []IT.ItemWithResult)

	//1. Nuskaito duomenu faila i lokalu masyva
	items := IT.ReadData("Data/IFF8-12_AkramasJ_L1_dat_1.json")
	//Giju skaicius : 2 <= x <= n/4 (n = 30)
	threadCount := 6
	//2. Paleidzia pasirinkta kieki giju
	var waitGroup = sync.WaitGroup{}
	waitGroup.Add(threadCount+2)
	for i := 0; i < threadCount; i++ {
		go DM.WorkProcess(&waitGroup, dataWorker, workerResult)
	}
	//2. Paleidzia vieną duomenu˛ masyvą valdanti˛ procesą;
	go DM.DataProcess(&waitGroup, len(items.Items), mainData, dataWorker)
	//2. Paleidzia vieną rezultatu˛ masyvą valdanti˛ procesą.
	go DM.ResultProcess(&waitGroup, len(items.Items), workerResult, resultMain)
	//3. . Duomenu˛ masyvą valdančiam procesui po vieną persiunčia visus nuskaitytus elementus iš failo.
	DM.ProvideItems(items)


	//4. Palaukia, kol visos paleistos gijos baigs darba
	waitGroup.Wait()
	//5. Atfiltruotus rezultatus isveda i tekstini faila
	//WriteData("Data/IFF8-12_AkramasJ_L1_rez.txt", resultMonitor.GetItems())
}
/*
func WriteData(fileName string, results []IT.ItemWithResult) {
	f, err := os.Create(fileName)
	if err != nil {
		log.Fatal(err)
	}
	defer f.Close()
	f.WriteString("-------------------------------------------------\n")
	f.WriteString("|Title               |Quantity|Price  |Result   |\n")
	f.WriteString("-------------------------------------------------\n")
	for i := 0; i < len(results); i++{
		_, err2 := f.WriteString(results[i].ToString())
		if err2 != nil {
			log.Fatal(err2)
		}
	}
	f.WriteString("-------------------------------------------------\n")

}*/