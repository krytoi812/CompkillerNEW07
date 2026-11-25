int lineTotal(int quantity, int price) {
    return quantity * price;
}

int deliveryCost() {
    return 50;
}

void showTotal(int total) {
    print total;
}

main {
    int books = lineTotal(readInt(), readInt());
    int pens = lineTotal(readInt(), readInt());
    showTotal(books + pens + deliveryCost());
}
