#include <string>
#include <iostream>
#include <omp.h>
#include <vector>
#include <nlohmann/json.hpp>
#include <fstream>
#include <iomanip>

using namespace std;
using json = nlohmann::json;

class Item {
public:
    string Title;
    int Quantity;
    float Price;

    Item() {}

    Item(string title, int quantity, float price) {
        this->Title = title;
        this->Quantity = quantity;
        this->Price = price;
    }

    string ToString() {
        char buff[100];
        snprintf(buff, sizeof(buff), "|%-20s|%-8d|%-7.2f", Title.c_str(), Quantity, Price);
        std::string buffAsStdStr = buff;
        return buffAsStdStr;
    }
};

class Items {
public:
    Item ItemArray[30];

    int size() {
        return sizeof(ItemArray) / sizeof(ItemArray[0]);
    }
};

class ItemWithResult {
public:
    Item Item;
    float Result;

    ItemWithResult() {}

    ItemWithResult(class Item it) {
        this->Item = it;
        this->Result = this->calculateValue();
    }

    float calculateValue() {
        vector<char> bytes(Item.Title.begin(), Item.Title.end());
        int stringValues = 0;
        for (char i : bytes) {
            stringValues += i;
        }
        int temp = stringValues ^Item.Quantity;
        float final = temp * Item.Price;
        return final;
    }

    string ToString() {
        char buff[100];
        snprintf(buff, sizeof(buff), "%s|%-9.2f|\n", Item.ToString().c_str(), Result);
        std::string buffAsStdStr = buff;
        return buffAsStdStr;
    }
};

class DataMonitor {
public:
    Item ItemArray[10];
    int currentSize;
    int length;
    int removed;
    bool workDone;
    omp_lock_t *lock = new omp_lock_t;

    DataMonitor() {
        this->currentSize = 0;
        this->length = 30;
        this->removed = 0;
        this->workDone = false;
        omp_init_lock(this->lock);
    }

    int addItem(Item item) {
        omp_set_lock(this->lock);
        if (currentSize > size() - 1) {
            omp_unset_lock(this->lock);
            return -1;
        } else {
            ItemArray[currentSize] = item;
            currentSize++;
        }
        omp_unset_lock(this->lock);
        return 0;
    }

    Item removeItem() {
        omp_set_lock(this->lock);
        Item item("", 0, -1);
        if (!workDone) {
            if (currentSize == 0) {
                omp_unset_lock(this->lock);
                return item;
            } else if (currentSize > 0) {
                item = ItemArray[currentSize - 1];
                currentSize--;
                removed++;
                if (removed == length) {
                    workDone = true;
                }
                omp_unset_lock(this->lock);
                return item;
            }
        }
        omp_unset_lock(this->lock);
        return item;
    }

    int size() {
        return sizeof(ItemArray) / sizeof(ItemArray[0]);
    }

    void ToString() {
        for (int i = 0; i < currentSize; i++) {
            cout << ItemArray[i].ToString() << endl;
        }
    }
};

class ResultMonitor {
public:
    ItemWithResult ItemArray[30];
    int length;
    int filtered;
    omp_lock_t *lock = new omp_lock_t;

    ResultMonitor() {
        this->length = 0;
        omp_init_lock(this->lock);
    }

    void addItemSorted(ItemWithResult item) {
        omp_set_lock(this->lock);
        length++;
        if (length == 1) {
            this->ItemArray[0] = item;
        } else if (item.Result > ItemArray[length - 2].Result) {
            ItemArray[length - 1] = item;
        } else {
            for (int i = 0; i < length - 1; i++) {
                if (item.Result < ItemArray[i].Result) {
                    for (int u = length - 1; u > i; u--) {
                        ItemArray[u] = ItemArray[u - 1];
                    }
                    ItemArray[i] = item;
                    break;
                }
            }
        }
        omp_unset_lock(this->lock);
    }

    ItemWithResult *GetItems() {
        ItemWithResult container[size()];
        int x = 0;
        for (int i = 0; i < size(); i++) {
            float value = ItemArray[i].Result;
            float precisionString = value - (int) value;
            if (precisionString > 0.5) {
                container[x] = ItemArray[i];
                x++;
            }
        }
        this->filtered = x;
        auto *finalContainer = new ItemWithResult[x];
        for (int i = 0; i < x; i++) {
            finalContainer[i] = container[i];
        }
        return finalContainer;
    }

    int size() {
        return sizeof(ItemArray) / sizeof(ItemArray[0]);
    }

};

static void readData(const string &dataFile, Item array[]) {
    vector<Item> itemsVector;
    json wholeFile;
    ifstream ifs(dataFile);
    ifs >> wholeFile;
    for (json::iterator i = wholeFile["items"].begin(); i != wholeFile["items"].end(); ++i) {
        itemsVector.emplace_back(i.value()["title"], i.value()["quantity"], i.value()["price"]);
    }
    for (int i = 0; i < itemsVector.size(); i++) {
        array[i] = itemsVector[i];
    }
    ifs.close();
}

static void writeData(const string &resultFile, ItemWithResult array[], int count) {
    ofstream ofs(resultFile);
    ofs << "-----------------------------------------------------\n";
    ofs << "|Nr.|Title               |Quantity|Price  |Result   |\n";
    ofs << "-----------------------------------------------------\n";
    for (int i = 0; i < count; i++) {
        ofs << "|" << setw(3) << i + 1 << setw(0) << array[i].ToString();
    }
    ofs << "-----------------------------------------------------\n";
    ofs.close();
}

void Work(DataMonitor *dataMonitor, ResultMonitor *resultMonitor) {
    bool done = false;
    while (!done) {
        Item removed = dataMonitor->removeItem();

        if (removed.Price == -1) {
            if (dataMonitor->workDone) {
                done = true;
            }
        } else {
            ItemWithResult itemWithResult(removed);
            resultMonitor->addItemSorted(itemWithResult);
        }
    }
}

void provideItems(Items array, DataMonitor *dataMonitor) {
    for (int i = 0; i < array.size(); i++) {
        if (dataMonitor->addItem(array.ItemArray[i]) == -1) {
            i--;
        }
    }
}

int main() {
    Items items;
    //1. Nuskaito duomenu faila i lokalu masyva
    readData("../Data/IFF8-12_AkramasJ_L1_dat_3.json", items.ItemArray);
    //Giju skaicius : 2 <= x <= n/4 (n = 30)
    int threads = 6;
    omp_set_num_threads(threads);
    //---
    DataMonitor dataMonitor;
    ResultMonitor resultMonitor;
    int threadNumber;
    //2. Paleidzia pasirinkta kieki giju
#pragma omp parallel default(none) private(threadNumber) shared(items, threads, dataMonitor, resultMonitor)
    {
        threadNumber = omp_get_thread_num();
        if (threadNumber + 1 == threads) {
            //3. I duomenu struktura is kurios gijos ims irasus iraso visus nuskaitytus duomenis
            provideItems(items, &dataMonitor);
        } else {
            Work(&dataMonitor, &resultMonitor);
        }
        //4. Palaukia, kol visos paleistos gijos baigs darba
    }
    //5. Atfiltruotus rezultatus isveda i tekstini faila
    ItemWithResult* results = resultMonitor.GetItems();
    writeData("../Data/IFF8-12_AkramasJ_L1_rez.txt", results, resultMonitor.filtered);
    omp_destroy_lock(dataMonitor.lock);
    omp_destroy_lock(resultMonitor.lock);
    return 0;
}