
uint TwoDToOneDIndex(uint2 id, uint Size)
{
    return (id.y * Size) + id.x;
}