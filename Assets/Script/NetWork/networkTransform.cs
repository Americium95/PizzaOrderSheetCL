using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class networkTransform : MonoBehaviour
{
    public float UpdateTime = 1;

    float dt = 0;

    public Text Console;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        dt += Time.deltaTime;
        transform.position = new Vector3(Mathf.Cos(Time.time) * 10, Mathf.Sin(Time.time) * 10, 10);
        transform.Rotate(new Vector3(0.25f, 0.5f, 1f));

        //�����ֱ�
        if (dt > UpdateTime)
        {
            dt = 0;

            //transform���� ��Ŷ ����
            List<byte> packet = new List<byte> { 0x01, 0x00 };
            //�����ѹ� ����
            packet.Add(0x00);
            packet.Add(0x01);
            //��ġ���� ����
            packet.AddRange(System.BitConverter.GetBytes(transform.position.x));
            packet.AddRange(System.BitConverter.GetBytes(transform.position.y));
            packet.AddRange(System.BitConverter.GetBytes(transform.position.z));
            //���� ����
            packet.Add((byte)((transform.eulerAngles.x % 360 + 360) % 360 * 0.7f));
            packet.Add((byte)((transform.eulerAngles.y % 360 + 360) % 360 * 0.7f));
            packet.Add((byte)((transform.eulerAngles.z % 360 + 360) % 360 * 0.7f));


            //ui������
            Console.text = 
                (
                "\n" +
                string.Join(' ', packet) +
                "\n" +
                "Px" + System.BitConverter.ToSingle(packet.ToArray(), 4) +
                "Py" + System.BitConverter.ToSingle(packet.ToArray(), 8) +
                "Pz" + System.BitConverter.ToSingle(packet.ToArray(), 12)+
                "\n" +
                "Rx" + packet[16] * 1.4f +
                "Ry" + packet[17] * 1.4f +
                "Rz" + packet[18] * 1.4f +
                Console.text
                );

            Console.text = Console.text.Substring(0, Mathf.Min(Console.text.Length, 500) );

            /*Debug.Log(
                string.Join(' ', packet)+
                "\n"+
                "x" + System.BitConverter.ToSingle(packet.ToArray(), 2) +
                "y" + System.BitConverter.ToSingle(packet.ToArray(), 6) +
                "z" + System.BitConverter.ToSingle(packet.ToArray(), 10)
            );*/

            //NetWork.bootstrapChannel.WriteAndFlushAsync( Encoding.UTF8.GetString(packet.ToArray()) );



            //����
            NetWork.send(packet);

        }

    }
}
