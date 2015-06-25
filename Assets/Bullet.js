// The speed the bullet moves 
   var Speed : float = 0; 
   
   // The number of seconds before the bullet is automatically destroyed 
   var SecondsUntilDestroy : float = 40; 
   
   private var startTime : float; 
   
   function Start () {
       startTime = Time.time; 
   } 
   
   function FixedUpdate () {
       // Move forward 
       //this.gameObject.transform.position += Speed * this.gameObject.transform.forward;
       
       // If the Bullet has existed as long as SecondsUntilDestroy, destroy it 
       if (Time.time - startTime >= SecondsUntilDestroy) {
           Destroy(this.gameObject);
       } 
   }
        