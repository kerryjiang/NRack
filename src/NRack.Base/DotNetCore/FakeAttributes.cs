namespace System.Runtime.Serialization
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Property)]
    public class SerializableAttribute : Attribute
    {
        
    }
    
    public class NonSerializedAttribute : Attribute
    {
        
    }
    
    [AttributeUsageAttribute(AttributeTargets.Method)]
    public class OnSerializingAttribute : Attribute
    {
        
    }
    
    [AttributeUsageAttribute(AttributeTargets.Method)]
    public class OnDeserializedAttribute : Attribute
    {
        
    }
    
    public class StreamingContext
    {
        
    }
}