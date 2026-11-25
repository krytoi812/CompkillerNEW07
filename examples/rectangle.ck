int rectangleArea(int x1, int y1, int x2, int y2) {
    int width = abs(x2 - x1);
    int height = abs(y2 - y1);
    return width * height;
}

main {
    int x1 = readInt();
    int y1 = readInt();
    int x2 = readInt();
    int y2 = readInt();
    print rectangleArea(x1, y1, x2, y2);
}
