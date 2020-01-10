using System.Collections;
using System.Collections.Generic;


public class MyPID
{
    public float Kp, Ki, Kd;

    private float lastError;

    private float P, I, D;


    public MyPID(float KpCoeff, float KiCoeff, float KdCoeff)
    {
        Kp = KpCoeff;
        Ki = KiCoeff;
        Kd = KdCoeff;
    }

    public float Update(float error, float dt)
    {
        P = error;
        I += error * dt;
        D = (error - lastError) / dt;
        lastError = error;

        float result = P * Kp + I * Ki + D * Kd;

        return result;
    }
}