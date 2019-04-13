/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 10.10.2016
 * Время: 16:32
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Collections;
using System.Reflection ;
using System.Collections.Generic;
using System.Text;
using gcodeparser.gcodes;


namespace gcodeparser
{
	/// <summary>
	/// MAchineStatus is a helper class to allows you to ask for feedrates or locations rather then requesting the variables directly from the machine status
	/// 
	/// User: rvt
	/// Date: 12/3/13
	/// Time: 9:03 AM
	/// </summary>
	public class MachineStatusHelper
	{

		internal MachineStatus machineStatus = new MachineStatus();


		public void setMachineStatus(MachineStatus machineStatus)
		{
			
				this.machineStatus = machineStatus;
		}


		public double getA()
		{
			
			return machineStatus.Coordinates[MachineStatus.Axis.A].Value;
		}

		public double getB()
		{
				
				return machineStatus.Coordinates[MachineStatus.Axis.B].Value;
		}

		public double getC()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.C].Value;
		}

		public double getU()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.U].Value;
		}

		public double getV()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.V].Value;
		}

		public double getW()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.W].Value;
		}

		public double getX()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.X].Value;
		}

		public double getY()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.Y].Value;
		}

		public double getZ()
		{

				return machineStatus.Coordinates[MachineStatus.Axis.Z].Value;
		}

		public double getMA()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.A].Value;
		}

		public double getMB()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.B].Value;
		}

		public double getMC()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.C].Value;
		}

		public double getMU()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.U].Value;
		}

		public virtual double getMV()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.V].Value;
		}

		public virtual double getMW()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.W].Value;
		}

		public virtual double getMX()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.X].Value;
		}

		public virtual double getMY()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.Y].Value;
		}

		public virtual double getMZ()
		{

				return machineStatus.MachineCoordinates[MachineStatus.Axis.Z].Value;
		}

		public virtual double getOMX()
		{

				return machineStatus.OriginalMachineCoordinates[MachineStatus.Axis.X].Value;
		}

		public virtual double getOMY()
		{

				return machineStatus.OriginalMachineCoordinates[MachineStatus.Axis.Y].Value;
		}

		public virtual double getOMZ()
		{

				return machineStatus.OriginalMachineCoordinates[MachineStatus.Axis.Z].Value;
		}		

		public virtual double getOA()
		{

				return machineStatus.MachineOffsets[MachineStatus.Axis.A].Value;
		}

		public virtual double getOB()
		{

				return machineStatus.MachineOffsets[MachineStatus.Axis.B].Value;
		}

		public virtual double getOC()
		{

				return machineStatus.MachineOffsets[MachineStatus.Axis.C].Value;
		}

		public virtual double getOU()
		{

				return machineStatus.MachineOffsets[MachineStatus.Axis.U].Value;
		}

		public virtual double getOV()
		{

				return machineStatus.MachineOffsets[MachineStatus.Axis.V].Value;
		}

		public virtual double getOW()
		{

				return machineStatus.MachineOffsets[MachineStatus.Axis.W].Value;
		}

		public virtual double getOX()
		{

				return machineStatus.OriginalCoordinates[MachineStatus.Axis.X].Value;
		}

		public virtual double getOY()
		{

				return machineStatus.OriginalCoordinates[MachineStatus.Axis.Y].Value;
		}

		public virtual double getOZ()
		{

				return machineStatus.OriginalCoordinates[MachineStatus.Axis.Z].Value;
		}
		
		public double getI()
		{

				return machineStatus.MachineCoordinatesVars[MachineStatus.NonModalsVars.I].Value;
		}

		public double getJ()
		{

				return machineStatus.MachineCoordinatesVars[MachineStatus.NonModalsVars.J].Value;
		}

		public double getK()
		{

				return machineStatus.MachineCoordinatesVars[MachineStatus.NonModalsVars.K].Value;
		}

		public double getR()
		{

				return machineStatus.MachineCoordinatesVars[MachineStatus.NonModalsVars.R].Value;
		}

		public double getCR()
		{

				return machineStatus.MachineCoordinatesVars[MachineStatus.NonModalsVars.CR].Value;
		}

		public double getTURN()
		{

				return machineStatus.MachineCoordinatesVars[MachineStatus.NonModalsVars.TURN].Value;
		}		

		public String getMotionMode()
		{
				
			return getModalValue(typeof(MotionMode.MotionModes).GetEnumNames()) ;

		}
		public String getDistanceMode()
		{
				
			return getModalValue(typeof(DistanceMode.DistanceModes).GetEnumNames()) ;

		}
		public String getCoordinateSystems()
		{
				
			return getModalValue(typeof(CoordinateSystemMode.CoordinateSystemModes).GetEnumNames()) ;

		}
		
		public String getActiveUnit()
		{

//				return getModalValue<Enum>(typeof(Units));
			return getModalValue(typeof(Units).GetEnumNames());

		}

		public virtual double getFeedrate()
		{

				return machineStatus.getModalVars()["F"].Value;
		}

		public T getModalValue<T>(T[] enumClass)
		{
			ISet<string> stat = machineStatus.getModals();
			foreach (T value in enumClass)
			{
				if (stat.Contains(value.ToString()))
				{					
					return value ;
				}
			}
			return default(T) ;
		}

		public String getActivePlane()
		{

	//			return getModalValue<Enum>(typeof(ActivePlane.ActPlane));
			return getModalValue(typeof(ActivePlane.ActPlane).GetEnumNames());
		}
		
		
	}

}
