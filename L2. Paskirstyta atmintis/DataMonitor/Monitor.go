package DataMonitor

import (
	IT "../Item"
	"fmt"
	"strconv"
)

func ProvideItems(items IT.Items, writeChan chan<- IT.Item, writeFlag chan<- int) {
	for _, item := range items.Items {
		writeFlag <- 1
		writeChan <- item
	}
}
func DataProcess(size int, readChan <-chan IT.Item, readFlag <-chan int, writeChan chan<- IT.Item, writeFlag <-chan int, threads int){
	container := make([]IT.Item, size/3)
	count := 0
	countAll := 0
	for countAll < size{
		if count >= len(container)-1{
			<-writeFlag
			count--
			countAll++
			writeChan <- container[count]
		} else if count <= 0{
			<- readFlag
			container[count] = <- readChan
			count++
		} else {
			select {
			case <- readFlag:
				container[count] = <- readChan
				count++
			case <- writeFlag:
				count--
				countAll++
				writeChan <- container[count]
			}
		}
	}
	for i := 0; i < threads; i++ {
		<-writeFlag
		writeChan <- IT.Item{Quantity: -1}
	}
}

func WorkProcess(readChan <-chan IT.Item, readFlag chan<- int, writeChan chan IT.ItemWithResult) {
	for {
		readFlag <- 1
		item := <-readChan
		if item.Quantity == -1{
			break
		}
		itemWithResult := IT.ItemWithResult{
			Item:   item,
			Result: item.CalculateValue(),
		}
		precisionString, _ := strconv.ParseFloat(fmt.Sprintf("%.2f", itemWithResult.Result - float64(int(itemWithResult.Result ))), 2)
		if precisionString > 0.5 {
			writeChan <- itemWithResult
		}
	}
	writeChan <- IT.ItemWithResult{Result: -1}
}

func ResultProcess(size int, readChan <-chan IT.ItemWithResult, writeChan chan<- []IT.ItemWithResult, threads int){
	container := make([]IT.ItemWithResult, size)
	endedThreads := 0
	count := 0
	for endedThreads < threads{
		item := <- readChan
		if item.Result == -1{
			endedThreads++
		} else {
			count++
			if count == 1 {
				container[0] = item
			}else if item.Result > container[count-2].Result{
				container[count-1] = item
			}else {
				for i := 0; i < count-1; i++ {
					if item.Result < container[i].Result {
						for u := count-1; u > i; u-- {
							container[u] = container[u-1]
						}
						container[i] = item
						break
					}
				}
			}
		}
	}
	resultContainer := make([]IT.ItemWithResult, count)
	for i := 0; i < count; i++ {
		resultContainer[i] = container[i]
	}
	writeChan <- resultContainer
}