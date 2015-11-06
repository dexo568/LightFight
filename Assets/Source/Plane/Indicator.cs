using UnityEngine;
using System.Collections;

public class Indicator : MonoBehaviour {
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

		// Camera to screen coordinates`
		float cameraXMax 			   = reference.rect.xMax;  // Percentage of screen?? FIXME
		float cameraXMin 			   = reference.rect.xMin;	
		float cameraXMinInScreenPixels = (cameraXMin * Screen.width) - (Screen.width * HALF);
		float cameraXMaxInScreenPixels = (cameraXMax * Screen.width) - (Screen.width * HALF);


		//TODO screenX is improperly scaled in single-player.
		screenX = (reference.WorldToViewportPoint(targetPosition).x * reference.pixelWidth)
			+ (cameraXMax * Screen.width) - Screen.width; 
		screenY = (reference.WorldToViewportPoint(targetPosition).y * reference.pixelHeight) 
			- (reference.pixelHeight * HALF);


		if (direction <= 0) {
			// Target is behind us

			//The below is more or less demon-code. Will try to comment it at some point.
			float cameraXCenterInScreenCoords = ((1.25f * cameraXMax) * Screen.width) - Screen.width;
			float xDistanceFromCenter = screenX - cameraXCenterInScreenCoords;
			screenX = cameraXCenterInScreenCoords - xDistanceFromCenter;
			if(Mathf.Abs(xDistanceFromCenter) > Mathf.Abs(screenY)){ //it's more to the left or right than up or down
				if(screenX < cameraXCenterInScreenCoords){
					screenX = cameraXMinInScreenPixels + indicatorX;
				}else{
					screenX = cameraXMaxInScreenPixels - indicatorX;
				}
				if (screenY < indicatorY - reference.pixelHeight * HALF) {
					screenY = indicatorY - reference.pixelHeight * HALF;
				} else if (screenY > reference.pixelHeight * HALF - indicatorY) {
					screenY = reference.pixelHeight * HALF - indicatorY;
				}
			}else{ //it's more to the top or bottom
				if(screenY <= 0){
					screenY = screenY = reference.pixelHeight * HALF - indicatorY;
				}else{
					screenY = indicatorY - reference.pixelHeight * HALF;
				}
				if (screenX < cameraXMinInScreenPixels + indicatorX) {
					screenX = cameraXMinInScreenPixels + indicatorX;
				} else if (screenX > cameraXMaxInScreenPixels - indicatorX) {
					screenX = cameraXMaxInScreenPixels - indicatorX;
				}
			}
		} else {
			// Target is in front of us
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
			// pixelWidth converts from viewport to screen space FIXME is this right???
			// Viewport is a percentage. reference.pixelWidth is width of one camera in pixels
			// Convert camera's max screen percentage to screen pixels, 
			// and then transform it over bc screen's origin is in the center/middle of the screen
		}

		// Checks if we're player 1 or player 2. Repositions x as necesary


		// Update the indicator's position
		indicator.anchoredPosition = new Vector2(screenX, screenY);
	}
}
