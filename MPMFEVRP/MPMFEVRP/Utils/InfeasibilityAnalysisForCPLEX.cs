using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;

public class InfeasibilityAnalysisForCPLEX
{
    public InfeasibilityAnalysisForCPLEX(string fileName)
    {
        try
        {
            Cplex cplex = new Cplex();
            cplex.ImportModel(fileName);
            IEnumerator matrixEnum = cplex.GetLPMatrixEnumerator();
            matrixEnum.MoveNext();
            ILPMatrix lp = (ILPMatrix)matrixEnum.Current;
            //Disable CPLEX logs
            cplex.SetOut(null);
            if (cplex.Solve())
            {
                System.Console.WriteLine("Model Feasible");
                System.Console.WriteLine("Solution status = " + cplex.GetStatus());
                System.Console.WriteLine("Solution value = " + cplex.ObjValue);
                double[] x = cplex.GetValues(lp);
                for (int j = 0; j < x.Length; ++j)
                    System.Console.WriteLine("Variable Name:" + lp.GetNumVar(j).Name + "; Value = " + x[j]);
            }
            else
            {
                System.Console.WriteLine("Solution status = " + cplex.GetStatus());
                System.Console.WriteLine("Model Infeasible, Calling CONFLICT REFINER");
                IRange[] rng = lp.Ranges;
                int numVars = 0;

                //calculate the number of non-boolean variables
                for (int c1 = 0; c1 < lp.NumVars.Length; c1++)
                    if (lp.GetNumVar(c1).Type != NumVarType.Bool)
                        numVars++;
                //find the number of SOSs in the model
                int numSOS = cplex.GetNSOSs();
                System.Console.WriteLine("Number of SOSs=" + numSOS);

                int numConstraints = rng.Length + 2 * numVars + numSOS;
                IConstraint[] constraints = new IConstraint[numConstraints];
                for (int c1 = 0; c1 < rng.Length; c1++)
                {
                    constraints[c1] = rng[c1];
                }
                int numVarCounter = 0;
                //add variable bounds to the constraints array
                for (int c1 = 0; c1 < lp.NumVars.Length; c1++)
                {
                    if (lp.GetNumVar(c1).Type != NumVarType.Bool)
                    {
                        constraints[rng.Length + 2 * numVarCounter] = cplex.AddLe(lp.GetNumVar(c1).LB, lp.GetNumVar(c1));
                        constraints[rng.Length + 2 * numVarCounter].Name = lp.GetNumVar(c1).ToString() + "_LB";
                        constraints[rng.Length + 2 * numVarCounter + 1] = cplex.AddGe(lp.GetNumVar(c1).UB, lp.GetNumVar(c1));
                        constraints[rng.Length + 2 * numVarCounter + 1].Name = lp.GetNumVar(c1).ToString() + "_UB";
                        numVarCounter++;
                    }
                }
                //add SOSs to the constraints array
                if (numSOS > 0)
                {
                    int s1Counter = 0;
                    IEnumerator s1 = cplex.GetSOS1Enumerator();
                    while (s1.MoveNext())
                    {
                        ISOS1 cur = (ISOS1)s1.Current;
                        System.Console.WriteLine(cur);
                        constraints[rng.Length + numVars * 2 + s1Counter] = (IConstraint)cur;
                        s1Counter++;
                    }
                    int s2Counter = 0;
                    IEnumerator s2 = cplex.GetSOS2Enumerator();
                    while (s2.MoveNext())
                    {
                        ISOS2 cur = (ISOS2)s2.Current;
                        System.Console.WriteLine(cur);
                        constraints[rng.Length + numVars * 2 + s1Counter + s2Counter] = (IConstraint)cur;
                        s2Counter++;
                    }
                }
                double[] prefs = new double[constraints.Length];
                for (int c1 = 0; c1 < constraints.Length; c1++)
                {
                    //System.Console.WriteLine(constraints[c1]);
                    prefs[c1] = 1.0;//change it per your requirements
                }
                if (cplex.RefineConflict(constraints, prefs))
                {
                    System.Console.WriteLine("Conflict Refinement process finished: Printing Conflicts");
                    Cplex.ConflictStatus[] conflict = cplex.GetConflict(constraints);
                    int numConConflicts = 0;
                    int numBoundConflicts = 0;
                    int numSOSConflicts = 0;
                    for (int c2 = 0; c2 < constraints.Length; c2++)
                    {
                        if (conflict[c2] == Cplex.ConflictStatus.Member)
                        {
                            System.Console.WriteLine(" Proved : " + constraints[c2]);
                            if (c2 < rng.Length)
                                numConConflicts++;
                            else if (c2 < rng.Length + 2 * numVars)
                                numBoundConflicts++;
                            else
                                numSOSConflicts++;

                        }
                        else if (conflict[c2] == Cplex.ConflictStatus.PossibleMember)
                        {
                            System.Console.WriteLine(" Possible : " + constraints[c2]);
                            if (c2 < rng.Length)
                                numConConflicts++;
                            else if (c2 < rng.Length + 2 * numVars)
                                numBoundConflicts++;
                            else
                                numSOSConflicts++;
                        }
                    }
                    System.Console.WriteLine("Conflict Summary:");
                    System.Console.WriteLine(" Constraint conflicts = " + numConConflicts);
                    System.Console.WriteLine(" Variable Bound conflicts = " + numBoundConflicts);
                    System.Console.WriteLine(" SOS conflicts = " + numSOSConflicts);
                }
                else
                {
                    System.Console.WriteLine("Conflict could not be refined");
                }
                System.Console.WriteLine("Calling FEASOPT");
                // cplex.SetParam(Cplex.IntParam.FeasOptMode, 0);//change per feasopt requirements
                // Relax contraints only, modify if variable bound relaxation is required
                double[] lb_pref = new double[rng.Length];
                double[] ub_pref = new double[rng.Length];
                for (int c1 = 0; c1 < rng.Length; c1++)
                {
                    lb_pref[c1] = 1.0;//change it per your requirements
                    ub_pref[c1] = 1.0;//change it per your requirements
                }
                if (cplex.FeasOpt(rng, lb_pref, ub_pref))
                {
                    System.Console.WriteLine("Finished Feasopt");
                    double[] infeas = cplex.GetInfeasibilities(rng);
                    //Print bound changes
                    System.Console.WriteLine("Suggested Bound changes:");
                    for (int c3 = 0; c3 < infeas.Length; c3++)
                        if (infeas[c3] != 0)
                            System.Console.WriteLine(" " + rng[c3] + " : Change=" + infeas[c3]);
                    System.Console.WriteLine("Relaxed Model's obj value=" + cplex.GetObjValue());
                    System.Console.WriteLine("Relaxed Model's solution status:" + cplex.GetCplexStatus());
                }
                else
                {
                    System.Console.WriteLine("FeasOpt failed- Could not repair infeasibilities");
                }
            }
            cplex.End();
        }
        catch (ILOG.Concert.Exception e)
        {
            System.Console.WriteLine("Concert exception caught: " + e);
        }
    }
}