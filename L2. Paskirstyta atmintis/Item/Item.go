package Item

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"os"
)

type ItemWithResult struct {
	Item   Item    `json:"items"`
	Result float64 `json:"result"`
}

func (item *ItemWithResult) ToString() string {
	return fmt.Sprintf("%s|%-9.2f|\n", item.Item.ToString(), item.Result)
}

type Items struct {
	Items []Item `json:"items"`
}

type Item struct {
	Title    string  `json:"title"`
	Quantity int     `json:"quantity"`
	Price    float64 `json:"price"`
}

func (item *Item) ToString() string {
	return fmt.Sprintf("|%-20s|%-8d|%-7.2f", item.Title, item.Quantity, item.Price)
}

func (item *Item) CalculateValue() float64 {
	var byteArray = []byte(item.Title)
	stringValues := 0
	for i := 0; i < len(byteArray); i++ {
		stringValues += int(byteArray[i])
	}
	var temp int = stringValues ^ item.Quantity
	var final float64 = float64(temp) * item.Price
	return final
}

func ReadData(fileName string) Items {
	dataFile, err := os.Open(fileName)
	if err != nil {
		fmt.Println(err)
	}
	defer dataFile.Close()

	readData, _ := ioutil.ReadAll(dataFile)

	var items Items

	json.Unmarshal(readData, &items)

	return items
}

