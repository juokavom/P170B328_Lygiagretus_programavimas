package DataMonitor

import (
	IT "../Item"
	"fmt"
	"strconv"
	"sync"
)
type SortedResultMonitor struct {
	container []IT.ItemWithResult
	Length    int
	mutex     *sync.Mutex
}
type DataMonitor struct {
	container   []IT.Item
	currentSize int
	length      int
	removed     int
	workDone    bool
	mutex       *sync.Mutex
	cond    	*sync.Cond
}

func (monitor *DataMonitor) ToString() string {
	return fmt.Sprintf(
		"container:\n%s, currentSize: %d," +
			"length: %d, removed: %d,  workdone: %v \n",
			monitor.ContainerString(), monitor.currentSize,
			monitor.length, monitor.removed, monitor.workDone)
}

func (monitor *DataMonitor) ContainerString() string{
	data := ""
	for _, element := range monitor.container{
		data += element.ToString() + "\n"
	}
	return data
}

func InitializeDM(size int) DataMonitor {
	var container = make([]IT.Item, size/3)
	var mutex = sync.Mutex{}
	var cond = sync.NewCond(&mutex)
	var dataMonitor = DataMonitor{container: container, workDone: false, length: size, removed: 0, mutex: &mutex, cond: cond}
	return dataMonitor
}

func InitializeSDM(size int) SortedResultMonitor {
	var container = make([]IT.ItemWithResult, size)
	var mutex = sync.Mutex{}
	var dataMonitor = SortedResultMonitor{container: container, Length: 0, mutex: &mutex}
	return dataMonitor
}

func ProvideItems(items IT.Items) {
	for _, item := range items.Items {
		monitor.addItem(item)
	}
}

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
}

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
}

func DataProcess(group *sync.WaitGroup, count int, readChan <-chan IT.Item, writeChan chan<- IT.Item){
	defer group.Done()
	dataMonitor := InitializeDM(count)
}

func ResultProcess(group *sync.WaitGroup, count int, readChan <-chan IT.ItemWithResult, writeChan chan<- []IT.ItemWithResult){
	defer group.Done()
	resultMonitor := InitializeSDM(count)

}

func WorkProcess(group *sync.WaitGroup, readChan <-chan IT.Item, writeChan chan<- IT.ItemWithResult) {
	defer group.Done()
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
	}
}

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
}
