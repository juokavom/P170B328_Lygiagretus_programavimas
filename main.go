package main

import (
	DM "./DataMonitor"
	IT "./Item"
	"log"
	"os"
	"sync"
)

func main() {
	//1. Nuskaito duomenu faila i lokalu masyva
	items := IT.ReadData("Data/IFF8-12_AkramasJ_L1_dat_1.json")
	//Giju skaicius : 2 <= x <= n/4 (n = 30)
	threadCount := 6
	dataMonitor := DM.InitializeDM(len(items.Items))
	resultMonitor := DM.InitializeSDM(len(items.Items))
	//2. Paleidzia pasirinkta kieki giju
	var waitGroup = sync.WaitGroup{}
	waitGroup.Add(threadCount)
	for i := 0; i < threadCount; i++ {
		go DM.Work(&dataMonitor, &resultMonitor, &waitGroup)
	}
	//3. I duomenu struktura is kurios gijos ims irasus iraso visus nuskaitytus duomenis
	dataMonitor.ProvideItems(items)
	//4. Palaukia, kol visos paleistos gijos baigs darba
	waitGroup.Wait()
	//5. Atfiltruotus rezultatus isveda i tekstini faila
	WriteData("Data/IFF8-12_AkramasJ_L1_rez.txt", resultMonitor.GetItems())
}

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

}