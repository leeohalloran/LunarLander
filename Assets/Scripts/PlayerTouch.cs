using UnityEngine;
using System.Collections;

public class PlayerTouch : MonoBehaviour {	
    //file:///C:/Apps/Unity/Editor/Data/Documentation/Documentation/Components/gui-Layout.html
    /// <summary> default to the bottom third of the screen </summary>
    public PlayerMoveEnum choice = PlayerMoveEnum.Undetermined;
	
    private void OnGUI()
    {
    }
	
	bool isPressed;
	void OnPress(bool isDown)
	{
		isPressed = isDown;
		
		if (isPressed == false)
			count = 0;
	}
	
	int count = 0;
	void Update()
	{
		if (isPressed
		    && Globals.BottomThruster != null
			&& Globals.LeftThruster != null
			&& Globals.RightThruster != null
			&& Globals.PlayerShip != null)
		{
			Thrusters thrusters = new Thrusters()
			{
				ThrusterBottom = Globals.BottomThruster,
				ThrusterLeft = Globals.LeftThruster,
				ThrusterRight = Globals.RightThruster,
			};
			Globals.PlayerShip.AddForce(thrusters.DoThrust(choice));
            print(choice.ToString() + " fire #" + count++);
		}
	}
}

public enum PlayerMoveEnum
{
	Undetermined,
	LeftThruster,
	RightThruster,
	LeftBottomThruster,
	BottomThruster,
	RightBottomThruster,
}