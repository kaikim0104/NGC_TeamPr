public interface IReceiver
{
    public void ApplyByteData(byte byteData);
    public void ApplyUShortData(ushort ushortData);
    public void ApplyByteData(byte byteData1, byte byteData2);
    public void ApplySbyteData(sbyte sbyteData);
    public void ApplySbyteData(sbyte sbyteData1, sbyte sbyteData2);
    public void ApplySbyteData(sbyte sbyteData1,sbyte sbyteData2, sbyte sbyteData3);
}
