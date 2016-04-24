using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PuzzleMaker
{

    /// <summary>
    /// Creates class to hold information for joints of piece
    /// </summary>
    public class SPieceInfo
    {
        private int _ID = 0;
        private List<SJointInfo> _Joints = new List<SJointInfo>();

        /// <summary>
        /// Returns id for this piece provided in constructor.
        /// </summary>
        public int ID { get { return _ID; } }

        /// <summary>
        /// Returns current amount of joints in this piece
        /// </summary>
        public int TotalJoints
        {
            get { return _Joints.Count; }
        }

        /// <summary>
        /// Gets joint information according to provided joint position
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Returns joint info, throws argument exception on piece not found</returns>
        public SJointInfo this[EJointPosition i]
        {

            get
            {
                foreach (SJointInfo item in _Joints)
                {
                    if (item.JointPosition == i)
                        return item;
                }

                throw new System.ArgumentException("Joint with provided JointPosition not found");
            }

        }


        /// <summary>
        /// Adds joint to this piece
        /// </summary>
        /// <param name="Joint">Joint info to add to this piece</param>
        /// <returns>Returns true if joint position provided in Joint is not already occupied returns false otherwise</returns>
        public bool AddJoint(SJointInfo Joint)
        {
            //Check if this joint type already exists
            foreach (SJointInfo item in _Joints)
                if (item.JointPosition == Joint.JointPosition)
                    return false;

            _Joints.Add(Joint);

            return true;
        }

        /// <summary>
        /// Creates piece information instance with provided piece id
        /// </summary>
        /// <param name="PieceID">Piece Id for this piece class instance, cannot be set after creation</param>
        public SPieceInfo(int PieceID)
        {
            _ID = PieceID;
        }

        /// <summary>
        /// Checks for existance of provided joint position
        /// </summary>
        /// <param name="Joint">Joint position to check for in this piece</param>
        /// <returns>Returns true if provided joint position is occupied by a joint</returns>
        public bool HaveJoint(EJointPosition Joint)
        {
            for (int i = 0; i < _Joints.Count; i++)
            {
                if (_Joints[i].JointPosition == Joint)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all joints information in this piece
        /// </summary>
        /// <returns>Returns array of joints information in this piece</returns>
        public SJointInfo[] GetJoints()
        {
            SJointInfo[] Result = new SJointInfo[_Joints.Count];
            _Joints.CopyTo(Result);
            return Result;
        }

        /// <summary>
        /// Returns joint information based on joint position.
        /// </summary>
        /// <param name="Joint">Joint position to return data for</param>
        /// <param name="IsFound">an out variable, true if joint is found false otherwise</param>
        /// <returns>Returns joint information of joint at provided joint position</returns>
        public SJointInfo GetJoint(EJointPosition Joint, out bool IsFound)
        {
            SJointInfo Result = new SJointInfo();

            foreach (SJointInfo item in _Joints)
            {
                if (item.JointPosition == Joint)
                {
                    Result = new SJointInfo(item.JointType, item.JointPosition, item.JointWidth, item.JointHeight);
                    IsFound = true;

                    return Result;
                }
            }

            IsFound = false;

            return Result;
        }

        /// <summary>
        /// Creates a deep copy of this class
        /// </summary>
        /// <returns>Returns a deep copy of this class</returns>
        public SPieceInfo MakeCopy()
        {
            SPieceInfo Temp = new SPieceInfo(_ID);
            foreach (SJointInfo item in _Joints)
                Temp.AddJoint(item);

            return Temp;
        }

        /// <summary>
        /// Returns string info with PieceID, and every joint string representation
        /// </summary>
        /// <returns>Returns string representation</returns>
        public override string ToString()
        {

            string Result = "";
            Result = "Piece ID : " + _ID.ToString() + "\r\n";
            foreach (SJointInfo item in _Joints)
                Result = Result + item.ToString() + "\r\n";


            return Result;
        }

    }

}
