using UnityEngine;
using System.Collections;

public class IndicatorDebug : MonoBehaviour {
	// One of the player's cameras
	public Camera reference;
	public GameObject tracked;
	const float HALF = .5f;
	
	void Start () {
		
	}
	
	// TODO Online multiplayer doesn't need splitscreen
	//      Remove portion of code that assumes splitscreen
	
	/**
	 * Get new screen x, y coordinates for the indicator
	 */
	void Update () {
		// Checks if target is behind us by doing: camera's normal (dot) vector to target 
		RectTransform indicator = (RectTransform) this.transform;
		Vector3 cameraPosition  = reference.transform.position;
		Vector3 targetPosition  = tracked.transform.position;
		Vector3 targetLine      = targetPosition - cameraPosition;
		float direction         = Vector3.Dot(reference.transform.forward,targetLine);
		float indicatorX 		= Mathf.Abs(indicator.rect.x); // rect.x returns negative, so correct it
		float indicatorY 		= Mathf.Abs(indicator.rect.y); // rect.y returns negative, so correct it
		float screenX, screenY;
		
		// Camera to screen coordinates
		float cameraXMax 			   = reference.rect.xMax;  // Percentage of screen?? FIXME
		float cameraXMin 			   = reference.rect.xMin;	
		float cameraXMinInScreenPixels = (cameraXMin * Screen.width) - (Screen.width * HALF);
		float cameraXMaxInScreenPixels = (cameraXMax * Screen.width) - (Screen.width * HALF);
		
		screenX = (reference.WorldToViewportPoint(targetPosition).x * reference.pixelWidth) 
			+ (cameraXMax * Screen.width) - Screen.width; 
		screenY = (reference.WorldToViewportPoint(targetPosition).y * reference.pixelHeight) 
			- (reference.pixelHeight * HALF);
		
		// Target is behind us 
		if (direction <= 0) {
	//		Debug.Log ("Behind");
	//		float cameraXCenterInScreenCoords = ((1.25f * cameraXMax) * Screen.width) - Screen.width;
	//		float xDistanceFromCenter = screenX - cameraXCenterInScreenCoords;
//			screenX = cameraXCenterInScreenCoords - xDistanceFromCenter;
//			screenY = -screenY;
//			if(Mathf.Abs(xDistanceFromCenter) > Mathf.Abs(screenY)){ //it's more to the left or right than up or down
//				if(screenX < cameraXCenterInScreenCoords){
//					screenX = cameraXMinInScreenPixels + indicatorX;
//				}else{
//					screenX = cameraXMaxInScreenPixels - indicatorX;
//				}
//			}else{ //it's more to the top or bottom
//				if(screenY <= 0){
//					screenY = screenY = reference.pixelHeight * HALF - indicatorY;
//				}else{
//					screenY = indicatorY - reference.pixelHeight * HALF;
//				}
//			}
			
			//			Vector3 viewPortTarget = reference.WorldToViewportPoint(targetPosition);
			//			float targetX = viewPortTarget.x; // as percentage of current camera
			//			float targetY = viewPortTarget.y; // as percentage of current camera
			//
			//			// X,Y is being projected to the wrong side for some reason
			//			float flippedTargetX = targetX - (2.0f * (HALF - targetX)); 
			//			float flippedTargetY = targetY - (2.0f * (HALF - targetY)); 
			//			Debug.Log (flippedTargetY);
			//
			//			// Test used to snap the indicator to the edge it's closer to
			//			float targetToHorzEdge = Mathf.Min(1.0f - flippedTargetY, flippedTargetY);
			//			float targetToVertEdge = Mathf.Min(1.0f - flippedTargetX, flippedTargetX);
			//
			//			// New indicator screen positions and snap axis it's closer to
			//			if (targetToVertEdge <= targetToHorzEdge) {
			//				// Closer to Verticle edges of screen, Snap x
			//				screenX = Mathf.Round(flippedTargetX) * reference.pixelWidth;
			//				screenY = flippedTargetY * reference.pixelHeight;
			//			} else {
			//				// Closer to horizontal edges of screen, Snap y
			//				screenX = flippedTargetX * reference.pixelWidth;
			//				screenY = Mathf.Round(flippedTargetY) * reference.pixelHeight;
			//			}
			//
			//			screenX += (cameraXMax * Screen.width) - Screen.width;
			//			screenY -= (reference.pixelHeight * HALF);
			
		} else {
			// Target is in front of us	 
			//Debug.Log ("In front");

			// pixelWidth converts from viewport to screen space FIXME is this right???
			// Viewport is a percentage. reference.pixelWidth is width of one camera in pixels
			// Convert camera's max screen percentage to screen pixels, 
			// and then transform it over bc screen's origin is in the center/middle of the screen
		}
		
		// Checks if we're player 1 or player 2. Repositions x as necesary
		if (screenX < cameraXMinInScreenPixels + indicatorX) {
			screenX = cameraXMinInScreenPixels + indicatorX;
		} else if (screenX > cameraXMaxInScreenPixels - indicatorX) {
			screenX = cameraXMaxInScreenPixels - indicatorX;
		}
		
		// Checks if we're player 1 or player 2. Repositions y as necesary
		if (screenY < indicatorY - reference.pixelHeight * HALF) {
			screenY = indicatorY - reference.pixelHeight * HALF;
		} else if (screenY > reference.pixelHeight * HALF - indicatorY) {
			screenY = reference.pixelHeight * HALF - indicatorY;
		}
		
		// Update the indicator's position
		indicator.anchoredPosition = new Vector2(screenX, screenY);
	}
}
