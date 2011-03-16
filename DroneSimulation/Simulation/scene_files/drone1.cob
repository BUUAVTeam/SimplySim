<?xml version="1.0"?>
<ComplexObject xmlns="http://www.simplysim.net/2009/12/datamodel.xsd">
  <Element Name="Body" Class="Actor" Type="drone1_files\drone1body">
    <Translation />
    <Rotation>
      <Quaternion W="1" />
    </Rotation>
  </Element>
  <Element Name="LeftBlade" Class="Actor" Type="drone1_files\righthandedblade">
    <Translation X="0.205060944" Y="0.045" Z="-0.205060974" />
    <Rotation>
      <Quaternion W="0.9238795" X="-2.23900565E-10" Y="0.382683456" Z="5.311849E-10" />
    </Rotation>
    <Scale />
  </Element>
  <Element Name="RightBlade" Class="Actor" Type="drone1_files\righthandedblade">
    <Translation X="-0.205060944" Y="0.045" Z="0.205060974" />
    <Rotation>
      <Quaternion W="0.9238795" X="-2.23900565E-10" Y="0.382683456" Z="5.311849E-10" />
    </Rotation>
    <Scale />
  </Element>
  <Element Name="RearBlade" Class="Actor" Type="drone1_files\lefthandedblade">
    <Translation X="0.205060974" Y="0.045" Z="0.205060944" />
    <Rotation>
      <Quaternion W="0.9171757" X="-6.670292E-10" Y="0.398482978" Z="7.08665848E-10" />
    </Rotation>
    <Scale />
  </Element>
  <Element Name="FrontBlade" Class="Actor" Type="drone1_files\lefthandedblade">
    <Translation X="-0.205060974" Y="0.045" Z="-0.205060944" />
    <Rotation>
      <Quaternion W="0.9238795" X="-2.23900565E-10" Y="0.382683456" Z="5.311849E-10" />
    </Rotation>
    <Scale />
  </Element>
  <MotorizedHingeJoint Name="LeftDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/LeftBlade" />
    <Second Path="/Body" />
    <Point X="1.40510792E-09" Y="-2.34939318E-10" Z="2.11103384E-08" />
    <Axis X="-2.682209E-07" Y="0.9999999" Z="4.28608018E-07" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="RightDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RightBlade" />
    <Second Path="/Body" />
    <Point X="-1.3321958E-09" Y="2.34939318E-10" Z="-2.1036513E-08" />
    <Axis X="-2.682209E-07" Y="0.9999999" Z="4.28608018E-07" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="FrontDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/FrontBlade" />
    <Second Path="/Body" />
    <Point X="2.10734257E-08" Y="2.37877024E-10" Z="-1.33173994E-09" />
    <Axis X="-2.58819256E-07" Y="0.9999999" Z="4.172325E-07" />
  </MotorizedHingeJoint>
  <MotorizedHingeJoint Name="RearDrive" TargetVelocity="0" MaxTorque="50">
    <First Path="/RearBlade" />
    <Second Path="/Body" />
    <Point X="-2.21321521E-08" Y="-5.10668563E-10" Z="5.492151E-09" />
    <Axis X="-2.73808837E-07" Y="0.9999999" Z="4.172325E-07" />
  </MotorizedHingeJoint>
</ComplexObject>