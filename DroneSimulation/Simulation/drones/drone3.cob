<?xml version="1.0"?>
<ComplexObject xmlns="http://www.simplysim.net/2009/12/datamodel.xsd">
  <Element Name="Body" Class="Actor" Type="drone3body">
    <Translation />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="FrontBlade" Class="Actor" Type="righthandedblade">
    <Translation Y="0.045" Z="-0.42" />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="FrontRightBlade" Class="Actor" Type="lefthandedblade">
    <Translation X="-0.365" Y="0.045" Z="-0.21" />
    <Rotation>
      <Quaternion W="0.258819073" Y="0.9659258" />
    </Rotation>
  </Element>
  <Element Name="RearBlade" Class="Actor" Type="lefthandedblade">
    <Translation Y="0.045" Z="0.42" />
    <Rotation>
      <Quaternion W="0.707106769" Y="0.707106769" />
    </Rotation>
  </Element>
  <Element Name="FrontLeftBlade" Class="Actor" Type="lefthandedblade">
    <Translation X="0.365" Y="0.045" Z="-0.21" />
    <Rotation>
      <Quaternion W="0.258819073" Y="-0.9659258" />
    </Rotation>
  </Element>
  <Element Name="RearLeftBlade" Class="Actor" Type="righthandedblade">
    <Translation X="0.365" Y="0.045" Z="0.21" />
    <Rotation>
      <Quaternion W="0.8660254" Y="0.5" />
    </Rotation>
  </Element>
  <Element Name="RearRightBlade" Class="Actor" Type="righthandedblade">
    <Translation X="-0.365" Y="0.045" Z="0.21" />
    <Rotation>
      <Quaternion W="0.8660254" Y="-0.5" />
    </Rotation>
  </Element>
  <MotorizedHingeJoint Name="RearLeftDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearLeftBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontLeftDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontLeftBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="RearRightDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearRightBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontRightDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontRightBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="RearDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
</ComplexObject>