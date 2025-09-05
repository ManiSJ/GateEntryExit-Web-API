namespace GateEntryExit.Domain.Manager
{
    public class SensorManager : ISensorManager
    {
        public SensorManager()
        {
            
        }

        public Sensor Create(Guid id, Guid gateId, string name)
        {
            return new Sensor(id, gateId, name);
        }
    }
}
