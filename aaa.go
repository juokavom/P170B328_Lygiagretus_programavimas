package main

import (
	"fmt"
	"math/rand"
)

func main() {
	var channel = make(chan int)
	var channel2 = make(chan int)
	var names = []string {"First", "Second", "Third", "Fourth", "Fifth"}
	var names2 = []string {"First111", "Second222", "Third333", "Fourth444", "Fifth555"}
	for _, name := range names {
		var message = rand.Intn(65535)
		go sender(channel, name, message)
	}
	for _, name := range names2 {
		var message = rand.Intn(65535)
		go sender(channel2, name, message)
	}
	for i := 0; i < len(names)*2; i++ {
		select {
		case <-channel:
			var message = <- channel
			fmt.Println("Received value 1", message)
		case <-channel2:
			var message = <- channel2
			fmt.Println("Received value 2", message)
		}
	}
}
func sender(channel chan<- int, name string, x int) {
	//fmt.Println(name, "is going to send value", x)
	channel <- x
}