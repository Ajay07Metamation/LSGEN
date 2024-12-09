/PROG  BENDSUB_VAC
/ATTR
OWNER		= MNEDITOR;
COMMENT		= "12:00 12-04";
PROG_SIZE	= 1994;
CREATE		= DATE 24-12-05  TIME 14:36:28;
MODIFIED	= DATE 24-12-06  TIME 15:09:42;
FILE_NAME	= TESTDEF;
VERSION		= 0;
LINE_COUNT	= 33;
MEMORY_SIZE	= 2494;
PROTECT		= READ_WRITE;
TCD:  STACK_SIZE	= 0,
      TASK_PRIORITY	= 50,
      TIME_SLICE	= 0,
      BUSY_LAMP_OFF	= 0,
      ABORT_REQUEST	= 0,
      PAUSE_REQUEST	= 0;
DEFAULT_GROUP	= 1,*,*,*,*;
CONTROL_CODE	= 00000000 00000000;
LOCAL_REGISTERS	= 0,0,0;
/APPL

LINE_TRACK;
  LINE_TRACK_SCHEDULE_NUMBER      : 0;
  LINE_TRACK_BOUNDARY_NUMBER      : 0;
  CONTINUE_TRACK_AT_PROG_END      : TRUE;

/MN
   1:  !,SYNCRO_BEND ;
   2:  UFRAME_NUM=2 ;
   3:  UTOOL_NUM=2 ;
   4:  R[15:SPD_J]=80    ;
   5:  FOR R[33]=1 TO 25 ;
   6:  PR[R[33]]=P[R[33]]    ;
   7:  ENDFOR ;
   8:  PR[75]=P[1]    ;
   9:  PR[75,3]=PR[75,3]+.5    ;
  10:  PR[61,3:LPOS_ABG]=PR[61,3:LPOS_ABG]-5    ;
  11:  DO[505:VAC_GRIP_ON]=ON ;
  12:  WAIT   1.50(sec) ;
  13:L PR[61:LPOS_ABG] 2000mm/sec FINE    ;
  14:J P[1] R[15:SPD_J]% CNT100    ;
  15:J PR[75] R[15:SPD_J]% CNT100    ;
  16:  WAIT    .25(sec) ;
  17:  DO[505:VAC_GRIP_ON]=OFF ;
  18:  WAIT   2.00(sec) ;
  19:  R[38]=0    ;
  20:  R[35]=0    ;
  21:  //DO[2:MOVE_TO_LDP]=ON ;
  22:  SKIP CONDITION R[35]<>R[34]    ;
  23:  LBL[2] ;
  24:  IF R[34]>=24,JMP LBL[3] ;
  25:  R[35]=R[34]    ;
  26:  DO[2:MOVE_TO_LDP]=ON ;
  27:J PR[R[35]] R[15:SPD_J]% CNT100 ACC35 Skip,LBL[2]    ;
  28:  JMP LBL[2] ;
  29:  R[38]=R[38]+1    ;
  30:  LBL[3] ;
  31:J P[25] R[15:SPD_J]% CNT100    ;
  32:J PR[73] R[15:SPD_J]% CNT100    ;
  33:  R[15:SPD_J]=20    ;
/POS
P[1]{
   GP1:
	UF : 1, UT : 2,	
	J1=    46.510 deg,	J2=    28.430 deg,	J3=   -42.670 deg,
	J4=   180.000 deg,	J5=  -132.670 deg,	J6=  -133.490 deg
};
P[2]{
   GP1:
	UF : 1, UT : 2,	
	J1=    46.830 deg,	J2=    27.840 deg,	J3=   -42.550 deg,
	J4=   178.180 deg,	J5=  -131.260 deg,	J6=  -134.390 deg
};
P[3]{
   GP1:
	UF : 1, UT : 2,	
	J1=    47.140 deg,	J2=    27.260 deg,	J3=   -42.410 deg,
	J4=   176.420 deg,	J5=  -129.810 deg,	J6=  -135.210 deg
};
P[4]{
   GP1:
	UF : 1, UT : 2,	
	J1=    47.450 deg,	J2=    26.680 deg,	J3=   -42.240 deg,
	J4=   174.720 deg,	J5=  -128.320 deg,	J6=  -135.960 deg
};
P[5]{
   GP1:
	UF : 1, UT : 2,	
	J1=    47.760 deg,	J2=    26.100 deg,	J3=   -42.050 deg,
	J4=   173.070 deg,	J5=  -126.790 deg,	J6=  -136.650 deg
};
P[6]{
   GP1:
	UF : 1, UT : 2,	
	J1=    48.070 deg,	J2=    25.520 deg,	J3=   -41.820 deg,
	J4=   171.470 deg,	J5=  -125.230 deg,	J6=  -137.260 deg
};
P[7]{
   GP1:
	UF : 1, UT : 2,	
	J1=    48.360 deg,	J2=    24.940 deg,	J3=   -41.570 deg,
	J4=   169.910 deg,	J5=  -123.630 deg,	J6=  -137.820 deg
};
P[8]{
   GP1:
	UF : 1, UT : 2,	
	J1=    48.650 deg,	J2=    24.370 deg,	J3=   -41.300 deg,
	J4=   168.400 deg,	J5=  -122.010 deg,	J6=  -138.310 deg
};
P[9]{
   GP1:
	UF : 2, UT : 2,	
	J1=    48.940 deg,	J2=    23.800 deg,	J3=   -41.000 deg,
	J4=   166.930 deg,	J5=  -120.350 deg,	J6=  -138.740 deg
};
P[10]{
   GP1:
	UF : 2, UT : 2,	
	J1=    49.220 deg,	J2=    23.240 deg,	J3=   -40.670 deg,
	J4=   165.490 deg,	J5=  -118.670 deg,	J6=  -139.110 deg
};
P[11]{
   GP1:
	UF : 2, UT : 2,	
	J1=    49.490 deg,	J2=    22.680 deg,	J3=   -40.320 deg,
	J4=   164.090 deg,	J5=  -116.970 deg,	J6=  -139.430 deg
};
P[12]{
   GP1:
	UF : 2, UT : 2,	
	J1=    49.750 deg,	J2=    22.130 deg,	J3=   -39.940 deg,
	J4=   162.710 deg,	J5=  -115.240 deg,	J6=  -139.690 deg
};
P[13]{
   GP1:
	UF : 2, UT : 2,	
	J1=    50.010 deg,	J2=    21.590 deg,	J3=   -39.540 deg,
	J4=   161.360 deg,	J5=  -113.490 deg,	J6=  -139.890 deg
};
P[14]{
   GP1:
	UF : 2, UT : 2,	
	J1=    50.260 deg,	J2=    21.060 deg,	J3=   -39.120 deg,
	J4=   160.030 deg,	J5=  -111.720 deg,	J6=  -140.050 deg
};
P[15]{
   GP1:
	UF : 2, UT : 2,	
	J1=    50.490 deg,	J2=    20.540 deg,	J3=   -38.680 deg,
	J4=   158.720 deg,	J5=  -109.930 deg,	J6=  -140.160 deg
};
P[16]{
   GP1:
	UF : 2, UT : 2,	
	J1=    50.720 deg,	J2=    20.030 deg,	J3=   -38.220 deg,
	J4=   157.420 deg,	J5=  -108.130 deg,	J6=  -140.210 deg
};
P[17]{
   GP1:
	UF : 2, UT : 2,	
	J1=    50.940 deg,	J2=    19.530 deg,	J3=   -37.730 deg,
	J4=   156.140 deg,	J5=  -106.310 deg,	J6=  -140.220 deg
};
P[18]{
   GP1:
	UF : 2, UT : 2,	
	J1=    51.150 deg,	J2=    19.050 deg,	J3=   -37.230 deg,
	J4=   154.870 deg,	J5=  -104.480 deg,	J6=  -140.180 deg
};
P[19]{
   GP1:
	UF : 2, UT : 2,	
	J1=    51.340 deg,	J2=    18.570 deg,	J3=   -36.710 deg,
	J4=   153.600 deg,	J5=  -102.640 deg,	J6=  -140.090 deg
};
P[20]{
   GP1:
	UF : 3, UT : 2,	
	J1=    51.530 deg,	J2=    18.120 deg,	J3=   -36.180 deg,
	J4=   152.340 deg,	J5=  -100.790 deg,	J6=  -139.960 deg
};
P[21]{
   GP1:
	UF : 3, UT : 2,	
	J1=    51.700 deg,	J2=    17.670 deg,	J3=   -35.620 deg,
	J4=   151.080 deg,	J5=   -98.940 deg,	J6=  -139.780 deg
};
P[22]{
   GP1:
	UF : 3, UT : 2,	
	J1=    51.860 deg,	J2=    17.250 deg,	J3=   -35.060 deg,
	J4=   149.820 deg,	J5=   -97.080 deg,	J6=  -139.550 deg
};
P[23]{
   GP1:
	UF : 3, UT : 2,	
	J1=    52.010 deg,	J2=    16.840 deg,	J3=   -34.480 deg,
	J4=   148.550 deg,	J5=   -95.220 deg,	J6=  -139.280 deg
};
P[24]{
   GP1:
	UF : 3, UT : 2,	
	J1=    52.140 deg,	J2=    16.450 deg,	J3=   -33.880 deg,
	J4=   147.270 deg,	J5=   -93.360 deg,	J6=  -138.960 deg
};
P[25]{
   GP1:
	UF : 3, UT : 2,	
	J1=    52.260 deg,	J2=    16.080 deg,	J3=   -33.280 deg,
	J4=   145.990 deg,	J5=   -91.500 deg,	J6=  -138.600 deg
};
/END
