using CitizenFX.Core;
using FivePD.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FivePD.API.Utils;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;

namespace MoveVehicle
{
    internal class MoveVehicle : Plugin
    {
        private Vehicle lastStoppedVehicle;
        private Ped lastDriver;
        private Vector3 lastDestination = Vector3.Zero;

        internal MoveVehicle() : base()
        {
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            // Check if the Q key is pressed
            if (Game.IsControlJustPressed(0, Control.Cover))
            {
                // Check if the player is performing a traffic stop
                if (Utilities.IsPlayerPerformingTrafficStop())
                {
                    // Get the vehicle and driver involved in the traffic stop
                    lastStoppedVehicle = Utilities.GetVehicleFromTrafficStop();
                    lastDriver = Utilities.GetDriverFromTrafficStop();

                    if (lastDriver.IsFleeing)
                    {
                        Screen.ShowNotification("You are in a pursit! They won't stop.");
                    }
                    else
                    {
                        if (lastStoppedVehicle != null && lastStoppedVehicle.Exists() && lastDriver != null && lastDriver.Exists())
                        {
                            // Clear all tasks from the driver
                            lastDriver.Task.ClearAll();
                            
                            // Calculate the new destination ahead of the vehicle
                            lastDestination = FindNextRoadsideLocation(lastStoppedVehicle);

                            if (lastDestination != Vector3.Zero)
                            {
                                Screen.ShowNotification("Vehicle is moving to another position.");

                                // Offset the destination position to the right by 2.0f units
                                lastDestination += lastStoppedVehicle.RightVector + 4f;

                                // Set a new driving task for the driver to the new destination
                                lastDriver.Task.DriveTo(lastStoppedVehicle, lastDestination, 1f, 7f, 447) ;

                            }else if(lastDestination == Vector3.Zero)
                            {
                                Screen.ShowNotification("Unable to find another safe space.");
                            }
                        }
                        else
                        {
                            Screen.ShowNotification("No vehicle or driver found for traffic stop.");
                        }
                    }                    
                }
                else
                {
                    Screen.ShowNotification("You are not performing a traffic stop.");
                }
            }
        }

        private Vector3 FindNextRoadsideLocation(Vehicle vehicle)
        {
            Vector3 currentPosition = vehicle.Position;
            Vector3 forwardDirection = vehicle.ForwardVector;

            // Calculate a point 50 units ahead of the vehicle
            Vector3 nextPosition = currentPosition + forwardDirection * 50.0f;

            // Find the closest vehicle node with a heading on the same side of the road
            if (GetClosestVehicleNodeWithHeading(nextPosition, out Vector3 closestNodePosition, out float roadHeading))
            {
                return closestNodePosition;
            }

            return Vector3.Zero; // No suitable location found
        }

        private bool GetClosestVehicleNodeWithHeading(Vector3 sourcePosition, out Vector3 closestNodePosition, out float roadHeading)
        {
            closestNodePosition = Vector3.Zero;
            roadHeading = 0.0f;

            // Call the native method to find the closest vehicle node with a heading
            return API.GetClosestVehicleNodeWithHeading(sourcePosition.X, sourcePosition.Y, sourcePosition.Z, ref closestNodePosition, ref roadHeading, 1, 3.0f, 0);
        }
    }
}


