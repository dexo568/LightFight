
var scrollSpeed_X = 0.5;
var scrollSpeed_Y = 0.5;
private var randomOffset;
function Start(){
	randomOffset = Random.Range(0,100)/100.0;
}
function Update() {
var offsetX = Time.time * scrollSpeed_X;
var offsetY = Time.time * scrollSpeed_Y + randomOffset;
print(randomOffset);
GetComponent.<Renderer>().material.mainTextureOffset = Vector2 (offsetX,offsetY);
}