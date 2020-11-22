package main

import "fmt"

func main(){
	senderReceiver := make(chan int)
	finish := make(chan int)
	receiverPrint1 := make(chan int)
	receiverPrint2 := make(chan int)
	endMain := make(chan int)
	go sender(0, senderReceiver, finish)
	go sender(11, senderReceiver, finish)
	go receiver(senderReceiver, finish, receiverPrint1, receiverPrint2)
	go printer(receiverPrint1, finish, "Pirmasis", endMain)
	go printer(receiverPrint2, finish, "Antrasis", endMain)

	<- endMain
	<- endMain
	fmt.Println("ended")
}

func sender(number int, out chan<- int, finish <-chan int){
	index := -1
	for index < 0{
		out <- number
		select {
		case <- finish:
			index = 1
		default:
			number++
		}
	}
}

func receiver(in <-chan int, finish chan<- int, print1 chan<- int, print2 chan<- int){
	for i := 0; i < 20; i++{
		value := <-in
		if value % 2 == 0{
			print1 <- value
		} else {
			print2 <- value
		}
	}
	for i := 0; i < 4; i++ {
		finish <- 1
	}
}

func printer(in <-chan int, finish <-chan int, name string, endMain chan<- int){
	array := make([]int, 20)
	count := 0

	index := -1
	for index < 0{
		select {
		case value := <-in:
			array[count] = value
			count++
		case <- finish:
			index = 1
		}
	}

	for i := 0; i < count; i++{
		fmt.Println("Reiksme - ", array[i], ".",name,"procesas spausdintojas.")
	}
	endMain <- 1
}