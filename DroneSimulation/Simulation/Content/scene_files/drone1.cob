<?xml version="1.0"?>
<ComplexObject xmlns="http://www.simplysim.net/2009/12/datamodel.xsd">
  <Element Name="Body" Class="Actor" Type="drone1_files\drone1body">
    <Translation />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="LeftBlade" Class="Actor" Type="common_files\righthandedblade">
    <Translation X="0.29" Y="0.045" />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="RightBlade" Class="Actor" Type="common_files\righthandedblade">
    <Translation X="-0.29" Y="0.045" />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="RearBlade" Class="Actor" Type="common_files\lefthandedblade">
    <Translation Y="0.045" Z="0.29" />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="FrontBlade" Class="Actor" Type="common_files\lefthandedblade">
    <Translation Y="0.045" Z="-0.29" />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <MotorizedHingeJoint Name="LeftDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/LeftBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="RightDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RightBlade" />
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
  <MotorizedHingeJoint Name="RearDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearBlade" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
</ComplexObject>