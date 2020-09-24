# Imports
import matplotlib.pyplot as plt
import numpy as np

import nidaqmx
from nidaqmx.stream_readers import AnalogMultiChannelReader
from nidaqmx import constants

# from nidaqmx import stream_readers  # not needed in this script
# from nidaqmx import stream_writers  # not needed in this script

import threading
import pickle
import datetime
import scipy.io

import socket
import time


#tcp socket 
HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 8081       # The port used by the server
BUFFER_SIZE = 1024

clicked = 0;
bounds = ""

plot_emg = True
recording = False

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((HOST, PORT))
s.setblocking(0)
s.settimeout(0.0001)
s.send(b'E_AWAKE')


# Parameters
sampling_freq_in = 1000  # in Hz
buffer_in_size = 10
bufsize_callback = buffer_in_size
buffer_in_size_cfg = round(buffer_in_size * 1)  # clock configuration
chans_in = 8  # set to number of active OPMs (x2 if By and Bz are used, but that is not recommended)
refresh_rate_plot = 10  # in Hz
crop = 0  # number of seconds to drop at acquisition start before saving
my_filename = 'test_3_opms'  # with full path if target folder different from current folder (do not leave trailing /)

pulse = 0
seconds_in_day = 24 * 60 * 60
curr_time = 0;

# Initialize data placeholders
buffer_in = np.zeros((chans_in, buffer_in_size))
data = np.zeros((chans_in + 2, 1))  # will contain a first column with zeros but that's fine
data_record = np.zeros((chans_in + 2, 1))

print(data)
# Definitions of basic functions
def ask_user():
    global running
    input("Press ENTER/RETURN to stop acquisition and coil drivers.")
    running = False


def cfg_read_task(acquisition):  # uses above parameters
    acquisition.ai_channels.add_ai_voltage_chan("Dev1/ai1:8", terminal_config = constants.TerminalConfiguration.RSE)  # has to match with chans_in
    acquisition.timing.cfg_samp_clk_timing(rate=sampling_freq_in, sample_mode=constants.AcquisitionType.CONTINUOUS,
                                           samps_per_chan=buffer_in_size_cfg)


def reading_task_callback(task_idx, event_type, num_samples, callback_data):  # bufsize_callback is passed to num_samples
    global data
    global buffer_in
    global pulse
    global curr_time
    global data_record

    if running:
        # It may be wiser to read slightly more than num_samples here, to make sure one does not miss any sample,
        # see: https://documentation.help/NI-DAQmx-Key-Concepts/contCAcqGen.html
        buffer_in = np.zeros((chans_in, num_samples))  # double definition ???
        stream_in.read_many_sample(buffer_in, num_samples, timeout=constants.WAIT_INFINITELY)
        
        buffer_in = np.insert(buffer_in, 0, pulse, axis=0)
        buffer_in = np.insert(buffer_in, 0, curr_time, axis=0)
        data = np.append(data, buffer_in, axis=1)  # appends buffered data to total variable data
        
        if recording:
            data_record = np.append(data_record, buffer_in, axis=1)

        pulse = 0
        

    return 0  # Absolutely needed for this callback to be well defined (see nidaqmx doc).


while True:

    try:
        recv_data = s.recv(BUFFER_SIZE).decode("utf-8")
        print(recv_data)
    except:
        recv_data = None



    # Configure and setup the tasks
    with nidaqmx.Task() as task_in:

 
        cfg_read_task(task_in)
        stream_in = AnalogMultiChannelReader(task_in.in_stream)
        task_in.register_every_n_samples_acquired_into_buffer_event(bufsize_callback, reading_task_callback)


        # Start threading to prompt user to stop
        thread_user = threading.Thread(target=ask_user)
        thread_user.start()


        # Main loop
        running = True
        
        task_in.start()

        f, (ax1, ax2, ax3, ax4, ax5, ax6, ax7, ax8) = plt.subplots(8, 1, sharex='all', sharey='none')

        while running:  # make this adapt to number of channels automatically
            # Plot a visual feedback for the user's mental health
            try:
                recv_data = s.recv(BUFFER_SIZE).decode("utf-8")
                print(recv_data)
            except:
                recv_data = None

            if(recv_data == "G_RUN"):
                first_time = datetime.datetime.now()
                recording = True
                time_start = datetime.datetime.now()

            if(recv_data == "G_STOP"):
                running = False

            if(recv_data == "pulse"):
                pulse = 1

            if(recording):
                later_time = datetime.datetime.now()
                difference = later_time - first_time
                curr_time = curr_time  + int(difference.microseconds/1000.0) + int(difference.seconds*1000.0)
                first_time = later_time

            if plot_emg:
                #s.send(bytes(str(data),"ascii"))
                ax1.clear()
                ax2.clear()
                ax3.clear()
                ax4.clear()
                ax5.clear()
                ax6.clear()
                ax7.clear()
                ax8.clear() 
                ax1.plot(data[2, -sampling_freq_in * 5:].T)  # 5 seconds rolling window
                ax2.plot(data[3, -sampling_freq_in * 5:].T)
                ax3.plot(data[4, -sampling_freq_in * 5:].T)
                ax4.plot(data[5, -sampling_freq_in * 5:].T)
                ax5.plot(data[6, -sampling_freq_in * 5:].T)
                ax6.plot(data[7, -sampling_freq_in * 5:].T)
                ax7.plot(data[8, -sampling_freq_in * 5:].T)
                ax8.plot(data[9, -sampling_freq_in * 5:].T)
                # Label and axis formatting
                ax8.set_xlabel('time [s]')
                #ax1.set_ylabel('voltage [V]')
                #ax2.set_ylabel('voltage [V]')
                #ax3.set_ylabel('voltage [V]')
                xticks = np.arange(0, data[0, -sampling_freq_in * 5:].size, sampling_freq_in)
                xticklabels = np.arange(0, xticks.size, 1)
                ax8.set_xticks(xticks)
                ax8.set_xticklabels(xticklabels)

                plt.pause(1/refresh_rate_plot)  # required for dynamic plot to work (if too low, nulling performance bad)


        # Close task to clear connection once done
        task_in.close()
        duration = datetime.datetime.now() - time_start
        break


# Final save data and metadata ... first in python reloadable format:
filename = my_filename
with open(filename, 'wb') as f:
    pickle.dump(data_record, f)
'''
Load this variable back with:
with open(name, 'rb') as f:
    data_reloaded = pickle.load(f)
'''
# Human-readable text file:
extension = '.txt'
np.set_printoptions(threshold=np.inf, linewidth=np.inf)  # turn off summarization, line-wrapping
with open(filename + extension, 'w') as f:
    f.write(str(time_start) + "\n")
    f.write("Frequency (Hz): " + str(sampling_freq_in) + "\n")
    f.write(np.array2string(data_record.T, separator=', '))  # improve precision here!
# Now in matlab:
extension = '.mat'
scipy.io.savemat(filename + extension, {'data':data_record})


# Some messages at the end
num_samples_acquired = data_record[0,:].size
print("\n")
print("OPM acquisition ended.\n")
print("Acquisition duration: {}.".format(duration))
print("Acquired samples: {}.".format(num_samples_acquired - 1))


# Final plot of whole time course the acquisition
plt.close('all')
f_tot, (ax1, ax2, ax3, ax4, ax5, ax6, ax7, ax8) = plt.subplots(8, 1, sharex='all', sharey='none')
ax1.plot(data_record[2, 10:].T)  # note the exclusion of the first 10 iterations (automatically zoomed in plot)
ax2.plot(data_record[3, 10:].T)
ax3.plot(data_record[4, 10:].T)
ax4.plot(data_record[5, 10:].T)
ax5.plot(data_record[6, 10:].T)
ax6.plot(data_record[7, 10:].T)
ax7.plot(data_record[8, 10:].T)
ax8.plot(data_record[9, 10:].T)
# Label formatting ...
ax8.set_xlabel('time [s]')
#ax1.set_ylabel('voltage [V]')
#ax2.set_ylabel('voltage [V]')
#ax3.set_ylabel('voltage [V]')
xticks = np.arange(0, data_record[0, :].size, sampling_freq_in)
xticklabels = np.arange(0, xticks.size, 1)
ax8.set_xticks(xticks)
ax8.set_xticklabels(xticklabels)
plt.show()