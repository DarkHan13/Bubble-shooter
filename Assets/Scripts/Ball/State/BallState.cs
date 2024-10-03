namespace Ball
{
    public class BallState
    {
        protected BallStateMachine StateMachine;
        
        public BallState(BallStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }
        
        public virtual void Update() 
        {
            
        }
        
        public virtual void Exit()
        {
            
        }

        public virtual void OnMouseDown()
        {
            
        }

        public virtual void OnMouseUp()
        {
            
        }

        public virtual void Enter()
        {
            
        }
    }
}