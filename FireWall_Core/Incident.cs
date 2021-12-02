

namespace FireWall_Core
{
    internal class Incident
    {

        private string _body;
        private const string _logName = "log.txt";
        public Incident(string body)
        { 
            _body = body;
        }
        public void  WriteToLog()
        {
            string line = String.Format("\n [{0}]  {1} ", DateTime.Now,_body);
            File.AppendAllText(_logName,line);

        }
    }
}
