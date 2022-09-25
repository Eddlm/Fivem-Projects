Scripts Physics. Yeah.

# Feature List

#### Inverse Torque
Negates vanilla the power drop off when sliding.
Server-wide. 

#### Custom Wheelies
Disables vanilla wheelies and implements a custom wheelie system. Can just disable wheelies too.
Server-wide. 
#### ESC
Reduces Inverse Torque's power if needed.
Server decides if clients can use the /esc command.
Works client-wide, not per car.

#### LCS
Modulates torque on launch to avoid wheelspin. In the future it will modulate actual throttle input.
Server decides if clients can use the /lcs command.
Works client-wide, not per car.

```
### Scripted Physics ###
# Inverse Torque
setr it_on 1 #Boolean

# IT measures slides as % of the car's fTractionCurveLateral.
# Default: multiplier will start from x1 at 50% to <it_mult_at_trlat_end> at 150%, uncapped.
setr it_trlat_start 50 #Percent of fTractionCurveLateral
setr it_trlat_end 150 #Percent of fTractionCurveLateral

setr it_mult_at_trlat_end 300 #Percent of car's power
setr it_max_mult 1000 #Percent of car's power
setr it_awd_penalty 0 #Percent

# ESC - Electronic Stability Control
setr esc_allow_client_toggle 1 #Boolean

# LCS - Launch Control System
setr lcs_allow_client_toggle 1 #Boolean

# Custom Wheelies
setr cw_disable_vanilla 1 #Boolean
setr cw_enable_custom_wheelies 1 #Boolean
setr cw_only_muscle 0 #Boolean

# To configure, assume it uses  fInitialDriveForce * 1000. So 0.2 = 200.
# To make all cars lift the same, use 999 and let the cw_power_max define the lift for every car.
setr cw_power_scale 100 #Percent

# 500 implies 0.5 is the maximum power considered.
setr cw_power_max 500 #Percent
```
