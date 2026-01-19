using System;

/// <summary>
/// واجهة أساسية لجميع رسائل الشبكة
/// تضمن تجانس الرسائل عبر النظام
/// </summary>
public interface INetworkMessage
{
    string GetMessageType();
    string SerializeToJson();
}

/// <summary>
/// غلاف معياري لرسائل WebSocket
/// يتبع بروتوكول السيرفر المحدد
/// </summary>
[Serializable]
public class WebSocketMessageWrapper
{
    public string type;
    public string data;

    public WebSocketMessageWrapper(string messageType, string messageData)
    {
        type = messageType;
        data = messageData;
    }
}

/// <summary>
أنواع الأحداث الشبكية المدعومة
يستخدم لتحديد نوع كل رسالة وكيفية معالجتها
/// </summary>
public enum NetworkEventType
{
    QueueStatus,
    MatchFound,
    MatchStart,
    GameSnapshot,
    GameEnd,
    Input
}