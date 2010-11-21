<?xml version="1.0"?>
<ComplexObject xmlns="http://www.simplysim.net/2009/12/datamodel.xsd">
  <Element Name="Body" Class="Actor" Type="drone2body">
    <Translation />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="FrontLeftBladeUp" Class="Actor" Type="righthandedblade">
    <Translation X="0.28" Y="0.045" Z="-0.16" />
    <Rotation>
      <Quaternion W="0.866" Y="-0.5" />
    </Rotation>
  </Element>
  <Element Name="FrontLeftBladeDown" Class="Actor" Type="lefthandedblade">
    <Translation X="0.28" Y="-0.045" Z="-0.16" />
    <Rotation>
      <Quaternion X="0.9659258" Z="-0.258819073" />
    </Rotation>
  </Element>
  <Element Name="FrontRightBladeUp" Class="Actor" Type="righthandedblade">
    <Translation X="-0.28" Y="0.045" Z="-0.16" />
    <Rotation>
      <Quaternion W="0.8660254" Y="0.5" />
    </Rotation>
  </Element>
  <Element Name="FrontRightBladeDown" Class="Actor" Type="lefthandedblade">
    <Translation X="-0.28" Y="-0.045" Z="-0.16" />
    <Rotation>
      <Quaternion X="0.707106769" Z="0.707106769" />
    </Rotation>
  </Element>
  <Element Name="RearBladeUp" Class="Actor" Type="righthandedblade">
    <Translation Y="0.045" Z="0.32" />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="RearBladeDown" Class="Actor" Type="lefthandedblade">
    <Translation Y="-0.045" Z="0.32" />
    <Rotation>
      <Quaternion X="0.9659258" Z="0.258819073" />
    </Rotation>
  </Element>
  <MotorizedHingeJoint Name="RearUpDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearBladeUp" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontLeftUpDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontLeftBladeUp" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontRightUpDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontRightBladeUp" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontRightDownDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontRightBladeDown" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="-1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontLeftDownDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontLeftBladeDown" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="-1" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="RearDownDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearBladeDown" />
    <Second Path="/Body" />
    <Point />
    <Axis Y="-1" />
  </MotorizedHingeJoint>
</ComplexObject>