using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleMaker
{

    /// <summary>
    /// Holds information for the joint of piece
    /// </summary>
    public struct SJointInfo
    {
        private EJointType _jointType;
        private EJointPosition _jointPosition;
        private int _jointWidth;
        private int _jointHeight;

        /// <summary>
        /// Returns type of joint as specified while creating this joint
        /// </summary>
        public EJointType JointType
        {
            get { return _jointType; }
        }

        /// <summary>
        /// Returns this joint position in piece as specified while creating this joint
        /// </summary>
        public EJointPosition JointPosition
        {
            get { return _jointPosition; }
        }

        /// <summary>
        /// Returns this joint width as specified while creating this joint
        /// </summary>
        public int JointWidth
        {
            get { return _jointWidth; }
        }


        /// <summary>
        /// Returns this joint height as specified while creating this joint
        /// </summary>
        public int JointHeight
        {
            get { return _jointHeight; }
        }


        /// <summary>
        /// Creates instance with provided joint information
        /// </summary>
        /// <param name="_JointType">Type of this joint</param>
        /// <param name="_JointPosition">Position of this joint in piece</param>
        /// <param name="_JointWidth">Width of this joint</param>
        /// <param name="_JointHeight">Height of this joint</param>
        public SJointInfo(EJointType _JointType, EJointPosition _JointPosition, int _JointWidth, int _JointHeight)
        {
            _jointType = _JointType;
            _jointPosition = _JointPosition;

            _jointWidth = _JointWidth;
            _jointHeight = _JointHeight;
        }

        /// <summary>
        /// Returns information of JointType, JointPosition, JointWidth, JointHeight in form of string
        /// </summary>
        /// <returns>Returns string generated representation of this structure</returns>
        public override string ToString()
        {
            string Result = "JointType : " + _jointType.ToString();
            Result = Result + "\r\n JointPosition : " + _jointPosition.ToString();
            Result = Result + "\r\n JointWidth : " + _jointWidth.ToString();
            Result = Result + "\r\n JointHeight : " + _jointHeight.ToString();
            Result = Result + "\r\n";

            return Result;
        }

    }

    /// <summary>
    /// Specifies type of joint
    /// </summary>
    public enum EJointType
    {
        Male = 0,
        Female = 1
    }


    /// <summary>
    /// Specifies types of joints position that joints can occupy
    /// </summary>
    public enum EJointPosition
    {
        Top = 0,
        Left = 1,
        Right = 2,
        Bottom = 3
    }

}
