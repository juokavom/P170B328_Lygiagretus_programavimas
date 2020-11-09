package main

import (
	DM "./DataMonitor"
	IT "./Item"
	"fmt"
	"log"
	"os"
)

func main() {
	mainData := make(chan IT.Item)
	mainFlagData := make(chan int)
	dataWorker := make(chan IT.Item)
	dataFlagWorker := make(chan int)
	workerResult := make(chan IT.ItemWithResult)
	resultMain := make(chan []IT.ItemWithResult)

	//1. Nuskaito duomenu faila i lokalu masyva
	items := IT.ReadData("Data/IFF8-12_AkramasJ_L1_dat_2.json")
	//Giju skaicius : 2 <= x <= n/4 (n = 30)
	threadCount := 6
	//2. Paleidzia pasirinkta kieki giju
	for i := 0; i < threadCount; i++ {
		go DM.WorkProcess(dataWorker, dataFlagWorker,workerResult)
	}
	//2. Paleidzia vieną duomenu˛ masyvą valdanti˛ procesą;
	go DM.DataProcess(len(items.Items), mainData, mainFlagData, dataWorker, dataFlagWorker, threadCount)
	//2. Paleidzia vieną rezultatu˛ masyvą valdanti˛ procesą.
	go DM.ResultProcess(len(items.Items), workerResult, resultMain, threadCount)
	//3. Duomenu˛ masyvą valdančiam procesui po vieną persiunčia visus nuskaitytus elementus iš failo.
	DM.ProvideItems(items, mainData, mainFlagData)
	//4. Sulaukia rezultatu is rezultatu proceso
	results := <- resultMain
	//5. Atfiltruotus rezultatus isveda i tekstini faila
	WriteData("Data/IFF8-12_AkramasJ_L1_rez.txt", results)
}

func WriteData(fileName string, results []IT.ItemWithResult) {
	f, err := os.Create(fileName)
	if err != nil {
		log.Fatal(err)
	}
	defer f.Close()
	f.WriteString("------------------------------------------------------\n")
	f.WriteString("|No. |Title               |Quantity|Price  |Result   |\n")
	f.WriteString("------------------------------------------------------\n")
	for i := 0; i < len(results); i++{
		nr := fmt.Sprintf("|%-4d%s", i+1, results[i].ToString())
		_, err2 := f.WriteString(nr)
		if err2 != nil {
			log.Fatal(err2)
		}
	}
	f.WriteString("------------------------------------------------------\n")

}