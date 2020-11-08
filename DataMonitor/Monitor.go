package DataMonitor

import (
	IT "../Item"
	"fmt"
	"sync"
)

func ProvideItems(items IT.Items, writeChan chan<- IT.Item, writeFlag chan<- int, end chan<- int) {
	for _, item := range items.Items {
		writeFlag <- 1
		writeChan <- item
	}
	end <- 1
}
/*
func (resultMonitor *SortedResultMonitor) addItemSorted(item IT.ItemWithResult){
	//surikiuoti
	resultMonitor.mutex.Lock()
	resultMonitor.Length++
	if resultMonitor.Length == 1 {
		resultMonitor.container[0] = item
	}else if item.Result > resultMonitor.container[resultMonitor.Length-2].Result{
		resultMonitor.container[resultMonitor.Length-1] = item
	}else {
		for i := 0; i < resultMonitor.Length-1; i++ {
			if item.Result < resultMonitor.container[i].Result {
				for u := resultMonitor.Length-1; u > i; u-- {
					resultMonitor.container[u] = resultMonitor.container[u-1]
				}
				resultMonitor.container[i] = item
				break
			}
		}
	}
	resultMonitor.mutex.Unlock()
}*/
/*
func (resultMonitor *SortedResultMonitor) GetItems() []IT.ItemWithResult{
	var container = make([]IT.ItemWithResult, len(resultMonitor.container))
	x := 0
	for i := 0; i < len(resultMonitor.container); i++ {
		value := resultMonitor.container[i].Result
		precisionString, _ := strconv.ParseFloat(fmt.Sprintf("%.2f", value - float64(int(value))), 2)
		if precisionString > 0.5 {
			container[x] = resultMonitor.container[i]
			x++
		}
	}
	finalContainer := make([]IT.ItemWithResult, x)
	for i := 0; i < x; i++ {
		finalContainer[i] = container[i]
	}
	return finalContainer
}*/

func DataProcess(group *sync.WaitGroup, size int, readChan <-chan IT.Item, readFlag <-chan int, writeChan chan<- IT.Item, writeFlag <-chan int, end <-chan int){
	defer group.Done()
	container := make([]IT.Item, size/3)
	count := 0
	for {
		if count >= len(container)-1{
			//fmt.Println("cia2")
			//Pilnas - nepriimti rasanciu (tik salinancius)
			<-writeFlag
			count--
			writeChan <- container[count]
		} else if count <= 0{
			//fmt.Println("cia")
			//Tuscias - nepriimti salinanciu (tik rasancius)
			<- readFlag
			container[count] = <- readChan
			count++
		} else {
			//fmt.Println("cia3")
			select {
			case <- readFlag:
				//fmt.Println("opa1")
				container[count] = <- readChan
				count++
			case <- writeFlag:
				//fmt.Println("opa2")
				count--
				writeChan <- container[count]
			case <- end:
				break
			}

		}

		/*for u := 0; u < len(container); u++ {
			fmt.Println(container[u])
		}*/
		//fmt.Println(count)
		//fmt.Println()


	}
	//fmt.Println("END OF STREAM")
}

func ResultProcess(group *sync.WaitGroup, size int, readChan <-chan IT.ItemWithResult, writeChan chan<- []IT.ItemWithResult){
	defer group.Done()
	//resultMonitor := InitializeSDM(count)
}

func WorkProcess(group *sync.WaitGroup, readChan <-chan IT.Item, readFlag chan<- int, writeChan chan IT.ItemWithResult, end <-chan int) {
	defer group.Done()
	for {
		readFlag <- 1
		item := <-readChan
		fmt.Println(item.ToString())
	}

	//fmt.Println("END OF WORK")
	/*
	done := false
	for !done {
		item, doneNow, last := monitor.removeItem()
		if !doneNow || last{
			result.addItemSorted(IT.ItemWithResult{
				Item:   item,
				Result: item.CalculateValue(),
			})
		}
		if doneNow || last {
			done = true
		}
	}*/
}
/*
func (monitor *DataMonitor) addItem(item IT.Item) {
	monitor.mutex.Lock()
	for monitor.currentSize > len(monitor.container)-1{
		monitor.cond.Wait()
	}
	monitor.container[monitor.currentSize] = item
	monitor.currentSize++
	monitor.cond.Broadcast()
	monitor.mutex.Unlock()
}

func (monitor *DataMonitor) removeItem() (IT.Item, bool, bool) {
	monitor.mutex.Lock()
	var item IT.Item
	last := false
	if !monitor.workDone{
		for monitor.currentSize == 0 {
			monitor.cond.Wait()
		}
		if monitor.currentSize > 0 {
			item = monitor.container[monitor.currentSize-1]
		}
		monitor.currentSize--
		monitor.removed++
		if monitor.removed == monitor.length {
			monitor.workDone = true
			last = true
		}
	}
	workDone := monitor.workDone
	monitor.cond.Broadcast()
	monitor.mutex.Unlock()
	return item, workDone, last
}*/