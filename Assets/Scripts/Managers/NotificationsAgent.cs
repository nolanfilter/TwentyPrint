using UnityEngine;
using UnityEngine.iOS;
using System.Collections;

public class NotificationsAgent : MonoBehaviour {

//	private const int twentyFourHours = 86400;
//	private const int seventyTwoHours = 259200;
//	private const int oneHundredSixtyEightHours = 604800;
//
//	public string oneDayNotification = "We miss you! Come make some cool prints!";
//	public string threeDayNotification = "We miss you! Come make some cool prints!";
//	public string sevenDayNotification = "We miss you! Come make some cool prints!";
//
//	private static NotificationsAgent mInstance;
//	public static NotificationsAgent instance
//	{
//		get
//		{
//			return mInstance;
//		}
//	}
//	
//	void Awake()
//	{
//		if( mInstance != null )
//		{
//			Debug.LogError( "Only one instance of NotificationsAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
//			Destroy( gameObject );
//			return;
//		}
//		
//		mInstance = this;
//	}
//
//	void Start()
//	{
//		UnityEngine.iOS.NotificationServices.RegisterForNotifications( NotificationType.Alert | NotificationType.Badge | NotificationType.Sound );
//		
//		//IOSNotificationController.instance.RegisterForRemoteNotifications( NotificationType.Alert | NotificationType.Badge | NotificationType.Sound );
//		
//		//IOSNotificationController.instance.addEventListener( IOSNotificationController.DEVICE_TOKEN_RECEIVED, OnTokenReceived );
//
//		ScheduleNotifications();
//	}
//
//	void OnApplicationPause( bool pauseStatus )
//	{
//		if( !pauseStatus )
//		{
//			ScheduleNotifications();
//		}
//	}
//
//	private void ScheduleNotifications()
//	{
//		IOSNotificationController.instance.CancelAllLocalNotifications();
//
//		//IOSNotificationController.instance.OnNotificationScheduleResult += OnNotificationScheduleResult;
//		
//		IOSNotificationController.instance.ScheduleNotification( twentyFourHours, oneDayNotification, true );
//		IOSNotificationController.instance.ScheduleNotification( seventyTwoHours, threeDayNotification, true );
//		IOSNotificationController.instance.ScheduleNotification( oneHundredSixtyEightHours, sevenDayNotification, true );
//	}
//
//	private void OnNotificationScheduleResult( ISN_Result res ) {
//		//IOSNotificationController.instance.OnNotificationScheduleResult -= OnNotificationScheduleResult;
//		string msg = string.Empty;
//		if(res.IsSucceeded) {
//			msg += "Notification was successfully scheduled \n allowed notifications types: \n";
//			if((IOSNotificationController.AllowedNotificationsType & IOSUIUserNotificationType.Alert) != 0) {
//				msg += "Alert ";
//			}
//			if((IOSNotificationController.AllowedNotificationsType & IOSUIUserNotificationType.Sound) != 0) {
//				msg += "Sound ";
//			}
//			if((IOSNotificationController.AllowedNotificationsType & IOSUIUserNotificationType.Badge) != 0) {
//				msg += "Badge ";
//			}
//		} else {
//			msg += "Notification scheduling failed";
//		}
//		
//		//IOSMessage.Create("On Notification Schedule Result", msg);
//	}
//	
//	private void OnTokenReceived( CEvent e )
//	{
//		IOSNotificationDeviceToken token = e.data as IOSNotificationDeviceToken;
//		
//		Debug.Log( "OnTokenReceived" );
//		
//		Debug.Log( token.tokenString );
//		
//		IOSNotificationController.instance.removeEventListener( IOSNotificationController.DEVICE_TOKEN_RECEIVED, OnTokenReceived );
//	}
}
